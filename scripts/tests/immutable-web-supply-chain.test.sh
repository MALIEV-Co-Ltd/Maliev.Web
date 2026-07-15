#!/usr/bin/env bash
set -euo pipefail

readonly repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)"
readonly temp_root="$(mktemp -d)"
trap 'rm -rf -- "$temp_root"' EXIT

mkdir -p "$temp_root/bin"
cat >"$temp_root/bin/crane" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail

case "$1" in
  digest)
    reference="$2"
    if [[ "$reference" == *@sha256:* ]]; then
      printf '%s\n' "${reference##*@}"
    elif [[ -f "$FAKE_TAG_STATE" ]]; then
      state="$(cat "$FAKE_TAG_STATE")"
      if [[ "$state" == "AUTH_ERROR" ]]; then
        echo "UNAUTHORIZED: registry lookup failed" >&2
        exit 1
      fi
      printf '%s\n' "$state"
    else
      echo "MANIFEST_UNKNOWN: requested tag was not found" >&2
      exit 1
    fi
    ;;
  copy)
    printf 'copy %s %s\n' "$2" "$3" >>"$FAKE_CRANE_LOG"
    printf '%s\n' "${2##*@}" >"$FAKE_TAG_STATE"
    ;;
  manifest)
    case "$2" in
      *@sha256:1111111111111111111111111111111111111111111111111111111111111111)
        attestation_subject="sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "orphan_subject" ]] && attestation_subject="sha256:eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"
        printf '{"manifests":[{"mediaType":"application/vnd.oci.image.manifest.v1+json","digest":"sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"},{"mediaType":"application/vnd.oci.image.manifest.v1+json","digest":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","annotations":{"vnd.docker.reference.type":"attestation-manifest","vnd.docker.reference.digest":"%s"}}]}\n' "$attestation_subject"
        ;;
      *@sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa)
        cat <<'JSON'
{"layers":[{"mediaType":"application/vnd.in-toto+json","digest":"sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc","annotations":{"in-toto.io/predicate-type":"https://spdx.dev/Document"}},{"mediaType":"application/vnd.in-toto+json","digest":"sha256:dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd","annotations":{"in-toto.io/predicate-type":"https://slsa.dev/provenance/v1"}}]}
JSON
        ;;
      *) exit 1 ;;
    esac
    ;;
  blob)
    case "$2" in
      *@sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc)
        subject="bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
        extra_subject=""
        [[ "${FAKE_ATTESTATION_MODE:-}" == "bad_subject" ]] && subject="eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "orphan_subject" ]] && subject="eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "extra_orphan_statement_subject" ]] && extra_subject=',{"name":"orphan","digest":{"sha256":"eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"}}'
        spdx_predicate='{"SPDXID":"SPDXRef-DOCUMENT","spdxVersion":"SPDX-2.3","dataLicense":"CC0-1.0","documentNamespace":"https://maliev.com/spdx/web/2222222222222222222222222222222222222222","name":"maliev-web","creationInfo":{"created":"2026-07-15T00:00:00Z","creators":["Tool: buildkit"]},"packages":[{"SPDXID":"SPDXRef-Package-web","name":"maliev-web","downloadLocation":"NOASSERTION"}]}'
        [[ "${FAKE_ATTESTATION_MODE:-}" == "empty_spdx" ]] && spdx_predicate='{}'
        [[ "${FAKE_ATTESTATION_MODE:-}" == "malformed_spdx" ]] && spdx_predicate='{"SPDXID":"SPDXRef-DOCUMENT","spdxVersion":"SPDX-2.3","dataLicense":"CC0-1.0","documentNamespace":"not-a-uri","packages":[]}'
        printf '{"_type":"https://in-toto.io/Statement/v0.1","subject":[{"name":"web","digest":{"sha256":"%s"}}%s],"predicateType":"https://spdx.dev/Document","predicate":%s}\n' "$subject" "$extra_subject" "$spdx_predicate"
        ;;
      *@sha256:dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd)
        subject="bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
        revision="2222222222222222222222222222222222222222"
        builder="https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/123456"
        source_uri="https://github.com/MALIEV-Co-Ltd/Maliev.Web.git"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "orphan_subject" ]] && subject="eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "bad_revision" ]] && revision="3333333333333333333333333333333333333333"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "decoy_revision" ]] && revision="3333333333333333333333333333333333333333"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "bad_builder" ]] && builder="https://example.invalid/builder"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "wrong_source_uri" ]] && source_uri="https://example.invalid/Maliev.Web.git"
        slsa_predicate=$(printf '{"buildDefinition":{"buildType":"https://github.com/moby/buildkit/blob/master/docs/attestations/slsa-definitions.md","externalParameters":{"configSource":{"uri":"%s#refs/heads/develop","digest":{"sha1":"%s"},"path":"Maliev.Web.Bff/Dockerfile"},"request":{"frontend":"dockerfile.v0"}},"internalParameters":{"builderPlatform":"linux/amd64"},"resolvedDependencies":[{"uri":"%s#refs/heads/develop","digest":{"sha1":"%s"}}]},"runDetails":{"builder":{"id":"%s"},"metadata":{"invocationId":"build-123","startedOn":"2026-07-15T00:00:00Z","finishedOn":"2026-07-15T00:01:00Z"}}}' "$source_uri" "$revision" "$source_uri" "$revision" "$builder")
        [[ "${FAKE_ATTESTATION_MODE:-}" == "empty_slsa" ]] && slsa_predicate='{}'
        [[ "${FAKE_ATTESTATION_MODE:-}" == "incomplete_slsa" ]] && slsa_predicate=$(printf '{"buildDefinition":{"buildType":"https://github.com/moby/buildkit/blob/master/docs/attestations/slsa-definitions.md","externalParameters":{},"resolvedDependencies":[]},"runDetails":{"builder":{"id":"%s"},"metadata":{}}}' "$builder")
        [[ "${FAKE_ATTESTATION_MODE:-}" == "missing_material" ]] && slsa_predicate=$(printf '{"buildDefinition":{"buildType":"https://github.com/moby/buildkit/blob/master/docs/attestations/slsa-definitions.md","externalParameters":{"configSource":{"uri":"https://github.com/MALIEV-Co-Ltd/Maliev.Web.git#refs/heads/develop","digest":{"sha1":"%s"},"path":"Maliev.Web.Bff/Dockerfile"},"request":{"frontend":"dockerfile.v0"}},"internalParameters":{"builderPlatform":"linux/amd64"},"resolvedDependencies":[{"uri":"pkg:docker/alpine@3.22","digest":{"sha256":"ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"}}]},"runDetails":{"builder":{"id":"%s"},"metadata":{"invocationId":"build-123","startedOn":"2026-07-15T00:00:00Z","finishedOn":"2026-07-15T00:01:00Z"}}}' "$revision" "$builder")
        [[ "${FAKE_ATTESTATION_MODE:-}" == "decoy_revision" ]] && slsa_predicate="${slsa_predicate%?},\"decoy\":{\"revision\":\"2222222222222222222222222222222222222222\"}}"
        printf '{"_type":"https://in-toto.io/Statement/v0.1","subject":[{"name":"web","digest":{"sha256":"%s"}}],"predicateType":"https://slsa.dev/provenance/v1","predicate":%s}\n' "$subject" "$slsa_predicate"
        ;;
      *) exit 1 ;;
    esac
    ;;
  *) exit 1 ;;
esac
EOF
chmod +x "$temp_root/bin/crane"
export PATH="$temp_root/bin:$PATH"
export FAKE_TAG_STATE="$temp_root/tag-state"
export FAKE_CRANE_LOG="$temp_root/crane.log"

readonly expected_digest="sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
bash "$repository_root/scripts/ensure-web-image-tag.sh" "registry.example/web@$expected_digest" "registry.example/web" "dev-0123456789ab" "$expected_digest"
test "$(wc -l <"$FAKE_CRANE_LOG")" -eq 1
bash "$repository_root/scripts/ensure-web-image-tag.sh" "registry.example/web@$expected_digest" "registry.example/web" "dev-0123456789ab" "$expected_digest"
test "$(wc -l <"$FAKE_CRANE_LOG")" -eq 1
printf '%s\n' 'sha256:ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff' >"$FAKE_TAG_STATE"
if bash "$repository_root/scripts/ensure-web-image-tag.sh" "registry.example/web@$expected_digest" "registry.example/web" "dev-0123456789ab" "$expected_digest"; then
  echo "Expected conflicting immutable tag to fail." >&2
  exit 1
fi
test "$(wc -l <"$FAKE_CRANE_LOG")" -eq 1
printf '%s\n' 'AUTH_ERROR' >"$FAKE_TAG_STATE"
if bash "$repository_root/scripts/ensure-web-image-tag.sh" "registry.example/web@$expected_digest" "registry.example/web" "dev-0123456789ab" "$expected_digest"; then
  echo "Expected registry lookup error to fail closed." >&2
  exit 1
fi
test "$(wc -l <"$FAKE_CRANE_LOG")" -eq 1

rm -f "$FAKE_TAG_STATE"
readonly attested_image="registry.example/web@sha256:1111111111111111111111111111111111111111111111111111111111111111"
readonly revision="2222222222222222222222222222222222222222"
bash "$repository_root/scripts/verify-web-image-attestations.sh" "$attested_image" "$revision" "https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/" "https://github.com/MALIEV-Co-Ltd/Maliev.Web.git"
for mode in bad_subject bad_revision bad_builder orphan_subject extra_orphan_statement_subject empty_spdx malformed_spdx empty_slsa incomplete_slsa missing_material decoy_revision wrong_source_uri; do
  if FAKE_ATTESTATION_MODE="$mode" bash "$repository_root/scripts/verify-web-image-attestations.sh" "$attested_image" "$revision" "https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/" "https://github.com/MALIEV-Co-Ltd/Maliev.Web.git"; then
    echo "Expected attestation mode $mode to fail." >&2
    exit 1
  fi
done

if ! command -v kustomize >/dev/null 2>&1 && [[ -x /mnt/c/Windows/System32/kustomize.exe ]]; then
  kustomize() { /mnt/c/Windows/System32/kustomize.exe "$@"; }
  export -f kustomize
fi
command -v kustomize >/dev/null 2>&1

readonly gitops_root="$temp_root/gitops"
readonly overlay="$gitops_root/3-apps/maliev-web/overlays/development"
mkdir -p "$gitops_root/3-apps/maliev-web/base" "$overlay"
cat >"$gitops_root/3-apps/maliev-web/base/kustomization.yaml" <<'EOF'
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
resources:
  - deployment.yaml
EOF
cat >"$gitops_root/3-apps/maliev-web/base/deployment.yaml" <<'EOF'
apiVersion: apps/v1
kind: Deployment
metadata:
  name: maliev-web
spec:
  selector:
    matchLabels:
      app: maliev-web
  template:
    metadata:
      labels:
        app: maliev-web
    spec:
      containers:
        - name: maliev-web
          image: asia-southeast1-docker.pkg.dev/maliev-website/maliev-website-artifact/maliev-web:latest
EOF
cat >"$overlay/health-patch.yaml" <<'EOF'
apiVersion: apps/v1
kind: Deployment
metadata:
  name: maliev-web
spec:
  template:
    spec:
      containers:
        - name: maliev-web
          readinessProbe:
            httpGet:
              path: /readiness
              port: 8080
EOF
cat >"$overlay/kustomization.yaml" <<'EOF'
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
resources:
  - ../../base
patches:
  - path: health-patch.yaml
  - path: build-metadata-patch.yaml
  - path: build-metadata-patch.yaml
namespace: maliev-dev
labels:
  - pairs:
      environment: development
EOF
git -C "$gitops_root" init -q
git -C "$gitops_root" config user.email test@example.com
git -C "$gitops_root" config user.name Test
git -C "$gitops_root" add .
git -C "$gitops_root" commit -qm fixture
bash "$repository_root/scripts/update-web-gitops-overlay.sh" "$gitops_root" development "asia-southeast1-docker.pkg.dev/maliev-website/maliev-web-artifact-dev/maliev-web" "$expected_digest"
test "$(grep -Ec '^[[:space:]]*-[[:space:]]*path:[[:space:]]*build-metadata-patch\.yaml[[:space:]]*$' "$overlay/kustomization.yaml")" -eq 1
grep -q 'environment: development' "$overlay/kustomization.yaml"

echo "Immutable Web supply-chain shell fixtures passed."
