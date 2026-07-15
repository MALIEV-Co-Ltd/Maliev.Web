#!/usr/bin/env bash
set -euo pipefail

if [[ "$#" -ne 3 ]]; then
  echo "Usage: $0 <image@sha256-digest> <source-revision> <trusted-builder-prefix>" >&2
  exit 2
fi

readonly image_reference="$1"
readonly source_revision="$2"
readonly trusted_builder_prefix="$3"
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

readonly index_manifest="$(crane manifest "$image_reference")"
mapfile -t attestation_references < <(
  python3 -c '
import json
import sys

index = json.load(sys.stdin)
for manifest in index.get("manifests", []):
    annotations = manifest.get("annotations", {})
    if annotations.get("vnd.docker.reference.type") == "attestation-manifest":
        print(f"{manifest.get('"'"'digest'"'"', '')}\t{annotations.get('"'"'vnd.docker.reference.digest'"'"', '')}")
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

  attestation_manifest="$(crane manifest "${image_repository}@${attestation_digest}")"
  mapfile -t statement_layers < <(
    python3 -c '
import json
import sys

manifest = json.load(sys.stdin)
for layer in manifest.get("layers", []):
    if layer.get("mediaType") == "application/vnd.in-toto+json":
        print(f"{layer.get('"'"'annotations'"'"', {}).get('"'"'in-toto.io/predicate-type'"'"', '')}\t{layer.get('"'"'digest'"'"', '')}")
' <<<"$attestation_manifest"
  )
  if [[ "${#statement_layers[@]}" -eq 0 ]]; then
    echo "Attestation manifest ${attestation_digest} contains no in-toto statements." >&2
    exit 1
  fi

  for statement_layer in "${statement_layers[@]}"; do
    IFS=$'\t' read -r annotated_predicate layer_digest <<<"$statement_layer"
    statement="$(crane blob "${image_repository}@${layer_digest}")"
    if ! python3 -c '
import json
import sys

statement = json.load(sys.stdin)
subject, predicate_type, source_revision, trusted_builder_prefix = sys.argv[1:]
statement_type = statement.get("_type")
if statement_type not in {"https://in-toto.io/Statement/v0.1", "https://in-toto.io/Statement/v1"}:
    raise SystemExit("unexpected in-toto statement type")
if statement.get("predicateType") != predicate_type:
    raise SystemExit("predicate annotation does not match statement")
if not any(item.get("digest", {}).get("sha256") == subject for item in statement.get("subject", [])):
    raise SystemExit("statement .subject does not match attestation subject")

if predicate_type == "https://slsa.dev/provenance/v1":
    if statement_type != "https://in-toto.io/Statement/v1":
        raise SystemExit("SLSA v1 provenance must use in-toto Statement/v1")
    # Required field: predicate.runDetails.builder.id
    builder_id = statement.get("predicate", {}).get("runDetails", {}).get("builder", {}).get("id", "")
    builder_run_id = builder_id.removeprefix(trusted_builder_prefix)
    if not builder_id.startswith(trusted_builder_prefix) or not builder_run_id.isdigit():
        raise SystemExit(f"unexpected builder identity: {builder_id}")

    def strings(value):
        if isinstance(value, str):
            yield value
        elif isinstance(value, dict):
            for child in value.values():
                yield from strings(child)
        elif isinstance(value, list):
            for child in value:
                yield from strings(child)

    if not any(source_revision in value for value in strings(statement)):
        raise SystemExit(f"source_revision {source_revision} is missing")
' "${subject_digest#sha256:}" "$annotated_predicate" "$source_revision" "$trusted_builder_prefix" <<<"$statement"; then
      echo "Attestation statement ${layer_digest} does not bind its declared predicate to subject ${subject_digest}." >&2
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
  echo "Image attestations must include both an SPDX SBOM and SLSA v1 provenance." >&2
  exit 1
fi

echo "Verified attestation subjects, source revision, builder identity, SPDX SBOM, and SLSA v1 provenance for ${image_reference}."
