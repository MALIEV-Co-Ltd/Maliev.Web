#!/usr/bin/env bash
set -euo pipefail

readonly repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)"
readonly workflow="$repository_root/.github/workflows/promote-production.yml"
readonly tag_helper="$repository_root/scripts/ensure-web-image-tag.sh"
readonly gitops_updater="$repository_root/scripts/update-web-gitops-overlay.sh"
readonly attestation_verifier="$repository_root/scripts/verify-web-image-attestations.sh"

for required_file in "$workflow" "$tag_helper" "$gitops_updater" "$attestation_verifier"; do
  test -f "$required_file"
done

grep -Fq "if: github.ref == 'refs/heads/main'" "$workflow"
grep -Fq 'environment: production' "$workflow"
grep -Fq 'cancel-in-progress: false' "$workflow"
grep -Fq 'maliev-web-artifact-staging/maliev-web' "$workflow"
grep -Fq 'maliev-web-artifact-prod/maliev-web' "$workflow"
grep -Fq "value(dockerConfig.immutableTags)" "$workflow"
grep -Fq 'test "$immutable_tags" = "True"' "$workflow"
grep -Fq 'test "$staging_digest" = "$APPROVED_DIGEST"' "$workflow"
grep -Fq 'ensure-web-image-tag.sh "${STAGING_IMAGE}@${APPROVED_DIGEST}" "$PRODUCTION_IMAGE" "$RELEASE_VERSION" "$APPROVED_DIGEST"' "$workflow"
grep -Fq 'test "$(crane digest "${PRODUCTION_IMAGE}:${RELEASE_VERSION}")" = "$APPROVED_DIGEST"' "$workflow"
grep -Fq 'update-web-gitops-overlay.sh maliev-gitops production' "$workflow"
grep -Fq 'git add -- 3-apps/maliev-web/overlays/production/kustomization.yaml 3-apps/maliev-web/overlays/production/build-metadata-patch.yaml' "$workflow"
if grep -Eiq 'docker/build-push-action|docker[[:space:]]+build|kubectl|argocd/environments' "$workflow"; then
  echo "Production promotion must not rebuild images or mutate deployment applications." >&2
  exit 1
fi

grep -Fq '3-apps/maliev-web/overlays/${environment}' "$gitops_updater"
grep -Fq 'Refusing change outside' "$gitops_updater"
grep -Fq 'https://in-toto.io/Statement/v0.1' "$attestation_verifier"
grep -Fq 'https://slsa.dev/provenance/v1' "$attestation_verifier"
if grep -Fq 'SLSA v1 provenance must use in-toto Statement/v1' "$attestation_verifier"; then
  echo "Verifier must accept the observed Statement/v0.1 wrapper for a SLSA v1 predicate." >&2
  exit 1
fi

readonly temp_root="$(mktemp -d)"
trap 'rm -rf -- "$temp_root"' EXIT
mkdir -p "$temp_root/bin"
cat >"$temp_root/bin/crane" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
case "$1" in
  digest) printf '%s\n' 'sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef' ;;
  copy) printf 'copy\n' >>"$FAKE_COPY_LOG" ;;
  *) exit 1 ;;
esac
EOF
chmod +x "$temp_root/bin/crane"
export PATH="$temp_root/bin:$PATH"
export FAKE_COPY_LOG="$temp_root/copy.log"

output="$(bash "$tag_helper" \
  'registry.example/staging/web@sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef' \
  'registry.example/production/web' \
  '1.2.3' \
  'sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef')"
grep -Fq 'no mutation required' <<<"$output"
test ! -e "$FAKE_COPY_LOG"

echo "Production promotion control fixture passed."
