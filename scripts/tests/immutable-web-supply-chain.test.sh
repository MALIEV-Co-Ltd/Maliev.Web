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
        cat <<'JSON'
{"manifests":[{"digest":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","annotations":{"vnd.docker.reference.type":"attestation-manifest","vnd.docker.reference.digest":"sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"}}]}
JSON
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
        [[ "${FAKE_ATTESTATION_MODE:-}" == "bad_subject" ]] && subject="eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"
        printf '{"_type":"https://in-toto.io/Statement/v0.1","subject":[{"name":"web","digest":{"sha256":"%s"}}],"predicateType":"https://spdx.dev/Document","predicate":{}}\n' "$subject"
        ;;
      *@sha256:dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd)
        subject="bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
        revision="2222222222222222222222222222222222222222"
        builder="https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/123456"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "bad_revision" ]] && revision="3333333333333333333333333333333333333333"
        [[ "${FAKE_ATTESTATION_MODE:-}" == "bad_builder" ]] && builder="https://example.invalid/builder"
        printf '{"_type":"https://in-toto.io/Statement/v1","subject":[{"name":"web","digest":{"sha256":"%s"}}],"predicateType":"https://slsa.dev/provenance/v1","predicate":{"buildDefinition":{"resolvedDependencies":[{"uri":"git+https://github.com/MALIEV-Co-Ltd/Maliev.Web","digest":{"gitCommit":"%s"}}]},"runDetails":{"builder":{"id":"%s"}}}}\n' "$subject" "$revision" "$builder"
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
bash "$repository_root/scripts/verify-web-image-attestations.sh" "$attested_image" "$revision" "https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/"
for mode in bad_subject bad_revision bad_builder; do
  if FAKE_ATTESTATION_MODE="$mode" bash "$repository_root/scripts/verify-web-image-attestations.sh" "$attested_image" "$revision" "https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/"; then
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
bash "$repository_root/scripts/update-web-gitops-overlay.sh" "$gitops_root" development "asia-southeast1-docker.pkg.dev/maliev-website/maliev-website-artifact-dev/maliev-web" "$expected_digest"
test "$(grep -Ec '^[[:space:]]*-[[:space:]]*path:[[:space:]]*build-metadata-patch\.yaml[[:space:]]*$' "$overlay/kustomization.yaml")" -eq 1
grep -q 'environment: development' "$overlay/kustomization.yaml"

echo "Immutable Web supply-chain shell fixtures passed."
