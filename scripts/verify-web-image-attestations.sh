#!/usr/bin/env bash
set -euo pipefail

if [[ "$#" -ne 4 ]]; then
  echo "Usage: $0 <image@sha256-digest> <source-revision> <trusted-builder-prefix> <expected-source-uri>" >&2
  exit 2
fi

readonly image_reference="$1"
readonly source_revision="$2"
readonly trusted_builder_prefix="$3"
readonly expected_source_uri="$4"
readonly image_repository="${image_reference%@*}"
readonly image_digest="${image_reference##*@}"

if [[ "$image_reference" != *@sha256:* ]] || [[ ! "$image_digest" =~ ^sha256:[0-9a-f]{64}$ ]]; then
  echo "Refusing non-digest image reference: $image_reference" >&2
  exit 2
fi

if [[ ! "$source_revision" =~ ^[0-9a-f]{40}$ ]]; then
  echo "Refusing invalid source revision: $source_revision" >&2
  exit 2
fi

if [[ "$trusted_builder_prefix" != "https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/" ]]; then
  echo "Refusing untrusted SLSA builder prefix: $trusted_builder_prefix" >&2
  exit 2
fi

if [[ "$expected_source_uri" != "https://github.com/MALIEV-Co-Ltd/Maliev.Web.git" ]]; then
  echo "Refusing unexpected source repository URI: $expected_source_uri" >&2
  exit 2
fi

readonly index_manifest="$(crane manifest "$image_reference")"
mapfile -t non_attestation_subjects < <(
  python3 -c '
import json
import sys

index = json.load(sys.stdin)
accepted_media_types = {
    "application/vnd.docker.distribution.manifest.v2+json",
    "application/vnd.oci.image.manifest.v1+json",
}
for manifest in index.get("manifests", []):
    annotations = manifest.get("annotations", {})
    if (
        manifest.get("mediaType") in accepted_media_types
        and annotations.get("vnd.docker.reference.type") != "attestation-manifest"
    ):
        print(manifest.get("digest", ""))
' <<<"$index_manifest"
)
if [[ "${#non_attestation_subjects[@]}" -eq 0 ]]; then
  echo "The published image index contains no non-attestation subject manifests." >&2
  exit 1
fi

declare -A published_subjects=()
for subject_digest in "${non_attestation_subjects[@]}"; do
  if [[ ! "$subject_digest" =~ ^sha256:[0-9a-f]{64}$ ]]; then
    echo "The published image index contains an invalid subject manifest digest." >&2
    exit 1
  fi
  published_subjects["$subject_digest"]=true
done

mapfile -t attestation_references < <(
  python3 -c '
import json
import sys

index = json.load(sys.stdin)
for manifest in index.get("manifests", []):
    annotations = manifest.get("annotations", {})
    if annotations.get("vnd.docker.reference.type") == "attestation-manifest":
        print(f"{manifest.get('"'"'digest'"'"', '"'"''"'"')}\t{annotations.get('"'"'vnd.docker.reference.digest'"'"', '"'"''"'"')}")
' <<<"$index_manifest"
)
if [[ "${#attestation_references[@]}" -eq 0 ]]; then
  echo "No embedded attestation manifests were found for ${image_reference}." >&2
  exit 1
fi

found_sbom=false
found_provenance=false
for attestation_reference in "${attestation_references[@]}"; do
  IFS=$'\t' read -r attestation_digest subject_digest <<<"$attestation_reference"
  if [[ ! "$attestation_digest" =~ ^sha256:[0-9a-f]{64}$ ]] || [[ ! "$subject_digest" =~ ^sha256:[0-9a-f]{64}$ ]]; then
    echo "Attestation manifest has an invalid digest or subject reference." >&2
    exit 1
  fi
  if [[ "${published_subjects[$subject_digest]:-}" != true ]]; then
    echo "Refusing orphan attestation subject ${subject_digest}; it is not a non-attestation manifest in ${image_digest}." >&2
    exit 1
  fi

  attestation_manifest="$(crane manifest "${image_repository}@${attestation_digest}")"
  mapfile -t statement_layers < <(
    python3 -c '
import json
import sys

manifest = json.load(sys.stdin)
for layer in manifest.get("layers", []):
    if layer.get("mediaType") == "application/vnd.in-toto+json":
        print(f"{layer.get('"'"'annotations'"'"', {}).get('"'"'in-toto.io/predicate-type'"'"', '"'"''"'"')}\t{layer.get('"'"'digest'"'"', '"'"''"'"')}")
' <<<"$attestation_manifest"
  )
  if [[ "${#statement_layers[@]}" -eq 0 ]]; then
    echo "Attestation manifest ${attestation_digest} contains no in-toto statements." >&2
    exit 1
  fi

  for statement_layer in "${statement_layers[@]}"; do
    IFS=$'\t' read -r annotated_predicate layer_digest <<<"$statement_layer"
    if [[ ! "$layer_digest" =~ ^sha256:[0-9a-f]{64}$ ]]; then
      echo "Attestation manifest ${attestation_digest} contains an invalid statement digest." >&2
      exit 1
    fi
    statement="$(crane blob "${image_repository}@${layer_digest}")"
    if ! python3 -c '
import json
import re
import sys
from urllib.parse import urlparse

statement = json.load(sys.stdin)
subject, predicate_type, source_revision, trusted_builder_prefix, expected_source_uri = sys.argv[1:]
statement_type = statement.get("_type")
if statement_type not in {"https://in-toto.io/Statement/v0.1", "https://in-toto.io/Statement/v1"}:
    raise SystemExit("unexpected in-toto statement type")
if statement.get("predicateType") != predicate_type:
    raise SystemExit("predicate annotation does not match statement")
statement_subjects = [item.get("digest", {}).get("sha256") for item in statement.get("subject", [])]
if statement_subjects != [subject]:
    raise SystemExit("statement .subject must contain only its index-bound attestation subject")

predicate = statement.get("predicate")
if not isinstance(predicate, dict):
    raise SystemExit("predicate must be an object")

if predicate_type == "https://spdx.dev/Document":
    if predicate.get("SPDXID") != "SPDXRef-DOCUMENT":
        raise SystemExit("SPDX document identifier is missing")
    if not re.fullmatch(r"SPDX-2\.[23]", predicate.get("spdxVersion", "")):
        raise SystemExit("unsupported SPDX version")
    if predicate.get("dataLicense") != "CC0-1.0":
        raise SystemExit("SPDX data license must be CC0-1.0")
    namespace = urlparse(predicate.get("documentNamespace", ""))
    if namespace.scheme not in {"http", "https"} or not namespace.netloc:
        raise SystemExit("SPDX document namespace must be an absolute URI")
    if not isinstance(predicate.get("name"), str) or not predicate["name"].strip():
        raise SystemExit("SPDX document name is missing")
    creation_info = predicate.get("creationInfo")
    if not isinstance(creation_info, dict) or not creation_info.get("created") or not creation_info.get("creators"):
        raise SystemExit("SPDX creation information is incomplete")
    packages = predicate.get("packages", [])
    files = predicate.get("files", [])
    if not isinstance(packages, list) or not isinstance(files, list) or not (packages or files):
        raise SystemExit("SPDX document contains no package or file inventory")
    if any(not item.get("SPDXID") or not item.get("name") for item in packages):
        raise SystemExit("SPDX package inventory is malformed")
    if any(not item.get("SPDXID") or not item.get("fileName") for item in files):
        raise SystemExit("SPDX file inventory is malformed")

if predicate_type == "https://slsa.dev/provenance/v1":
    if statement_type != "https://in-toto.io/Statement/v1":
        raise SystemExit("SLSA v1 provenance must use in-toto Statement/v1")
    build_definition = predicate.get("buildDefinition")
    if not isinstance(build_definition, dict):
        raise SystemExit("SLSA buildDefinition is missing")
    build_type = build_definition.get("buildType", "")
    if build_type != "https://github.com/moby/buildkit/blob/master/docs/attestations/slsa-definitions.md":
        raise SystemExit("unexpected SLSA buildType")
    if not isinstance(build_definition.get("externalParameters"), dict) or not build_definition["externalParameters"]:
        raise SystemExit("SLSA externalParameters are missing")
    if not isinstance(build_definition.get("internalParameters"), dict) or not build_definition["internalParameters"]:
        raise SystemExit("SLSA internalParameters are missing")
    resolved_dependencies = build_definition.get("resolvedDependencies")
    if not isinstance(resolved_dependencies, list) or not resolved_dependencies:
        raise SystemExit("SLSA resolvedDependencies are missing")
    source_materials = [
        dependency
        for dependency in resolved_dependencies
        if isinstance(dependency, dict)
        and dependency.get("uri", "").partition("#")[0] == expected_source_uri
    ]
    if not any(material.get("digest", {}).get("sha1") == source_revision for material in source_materials):
        raise SystemExit("expected Git source material and revision are missing")

    run_details = predicate.get("runDetails")
    if not isinstance(run_details, dict):
        raise SystemExit("SLSA runDetails are missing")
    builder_id = run_details.get("builder", {}).get("id", "")
    builder_run_id = builder_id.removeprefix(trusted_builder_prefix)
    if not builder_id.startswith(trusted_builder_prefix) or not builder_run_id.isdigit():
        raise SystemExit(f"unexpected builder identity: {builder_id}")
    metadata = run_details.get("metadata")
    if not isinstance(metadata, dict):
        raise SystemExit("SLSA invocation metadata is missing")
    for field in ("invocationId", "startedOn", "finishedOn"):
        if not isinstance(metadata.get(field), str) or not metadata[field].strip():
            raise SystemExit(f"SLSA metadata.{field} is missing")
' "${subject_digest#sha256:}" "$annotated_predicate" "$source_revision" "$trusted_builder_prefix" "$expected_source_uri" <<<"$statement"; then
      echo "Attestation statement ${layer_digest} does not bind a valid ${annotated_predicate} predicate to subject ${subject_digest}." >&2
      exit 1
    fi

    case "$annotated_predicate" in
      https://spdx.dev/Document)
        found_sbom=true
        ;;
      https://slsa.dev/provenance/v1)
        found_provenance=true
        ;;
    esac
  done
done

if [[ "$found_sbom" != true ]] || [[ "$found_provenance" != true ]]; then
  echo "Image attestations must include both a structurally valid SPDX SBOM and SLSA v1 provenance." >&2
  exit 1
fi

echo "Verified index-bound attestation subjects, source material, builder identity, SPDX SBOM, and SLSA v1 provenance for ${image_reference}."
