#!/usr/bin/env bash
set -euo pipefail

readonly repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)"
readonly workflow="$repository_root/.github/workflows/promote-production.yml"
readonly validation_workflow="$repository_root/.github/workflows/production-control-validation.yml"
readonly tag_helper="$repository_root/scripts/ensure-web-image-tag.sh"
readonly gitops_updater="$repository_root/scripts/update-web-gitops-overlay.sh"
readonly attestation_verifier="$repository_root/scripts/verify-web-image-attestations.sh"

for required_file in "$workflow" "$validation_workflow" "$tag_helper" "$gitops_updater" "$attestation_verifier"; do
  test -f "$required_file"
done

grep -Fq 'pull_request:' "$validation_workflow"
grep -Fq 'permissions:' "$validation_workflow"
grep -Fq 'contents: read' "$validation_workflow"
grep -Fq 'group: production-control-validation-' "$validation_workflow"
grep -Fq 'cancel-in-progress: true' "$validation_workflow"
grep -Fq 'actions/checkout@9c091bb21b7c1c1d1991bb908d89e4e9dddfe3e0' "$validation_workflow"
grep -Fq 'bash -n scripts/ensure-web-image-tag.sh' "$validation_workflow"
grep -Fq 'bash scripts/tests/production-promotion-controls.test.sh' "$validation_workflow"
for validated_path in \
  '.github/workflows/promote-production.yml' \
  '.github/workflows/production-control-validation.yml' \
  'scripts/ensure-web-image-tag.sh' \
  'scripts/update-web-gitops-overlay.sh' \
  'scripts/verify-web-image-attestations.sh' \
  'scripts/tests/production-promotion-controls.test.sh'; do
  grep -Fq -- "- $validated_path" "$validation_workflow"
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
  digest)
    if [[ "$2" == *@sha256:* ]]; then
      printf '%s\n' "${2##*@}"
    elif [[ "${FAKE_TAG_MODE:-same}" == "conflict" ]]; then
      printf '%s\n' 'sha256:ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff'
    else
      printf '%s\n' 'sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef'
    fi
    ;;
  copy)
    printf 'copy\n' >>"$FAKE_COPY_LOG"
    ;;
  manifest)
    case "$2" in
      *@sha256:1111111111111111111111111111111111111111111111111111111111111111)
        subject='sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
        [[ "${FAKE_ATTESTATION_MODE:-valid}" == "orphan" ]] && subject='sha256:eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee'
        printf '{"manifests":[{"mediaType":"application/vnd.oci.image.manifest.v1+json","digest":"sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"},{"mediaType":"application/vnd.oci.image.manifest.v1+json","digest":"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","annotations":{"vnd.docker.reference.type":"attestation-manifest","vnd.docker.reference.digest":"%s"}}]}\n' "$subject"
        ;;
      *@sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa)
        printf '%s\n' '{"layers":[{"mediaType":"application/vnd.in-toto+json","digest":"sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc","annotations":{"in-toto.io/predicate-type":"https://spdx.dev/Document"}},{"mediaType":"application/vnd.in-toto+json","digest":"sha256:dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd","annotations":{"in-toto.io/predicate-type":"https://slsa.dev/provenance/v1"}}]}'
        ;;
      *) exit 1 ;;
    esac
    ;;
  blob)
    case "$2" in
      *@sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc)
        predicate='{"SPDXID":"SPDXRef-DOCUMENT","spdxVersion":"SPDX-2.3","dataLicense":"CC0-1.0","documentNamespace":"https://maliev.com/spdx/web/2222222222222222222222222222222222222222","name":"maliev-web","creationInfo":{"created":"2026-07-15T00:00:00Z","creators":["Tool: buildkit"]},"packages":[{"SPDXID":"SPDXRef-Package-web","name":"maliev-web","downloadLocation":"NOASSERTION"}]}'
        [[ "${FAKE_ATTESTATION_MODE:-valid}" == "tampered" ]] && predicate='{}'
        printf '{"_type":"https://in-toto.io/Statement/v0.1","subject":[{"name":"web","digest":{"sha256":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"}}],"predicateType":"https://spdx.dev/Document","predicate":%s}\n' "$predicate"
        ;;
      *@sha256:dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd)
        printf '%s\n' '{"_type":"https://in-toto.io/Statement/v0.1","subject":[{"name":"web","digest":{"sha256":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"}}],"predicateType":"https://slsa.dev/provenance/v1","predicate":{"buildDefinition":{"buildType":"https://github.com/moby/buildkit/blob/master/docs/attestations/slsa-definitions.md","externalParameters":{"configSource":{"uri":"https://github.com/MALIEV-Co-Ltd/Maliev.Web.git#refs/heads/main","digest":{"sha1":"2222222222222222222222222222222222222222"},"path":"Dockerfile"},"request":{"frontend":"dockerfile.v0"}},"internalParameters":{"builderPlatform":"linux/amd64"},"resolvedDependencies":[{"uri":"https://github.com/MALIEV-Co-Ltd/Maliev.Web.git#refs/heads/main","digest":{"sha1":"2222222222222222222222222222222222222222"}}]},"runDetails":{"builder":{"id":"https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/123456"},"metadata":{"invocationId":"build-123","startedOn":"2026-07-15T00:00:00Z","finishedOn":"2026-07-15T00:01:00Z"}}}}'
        ;;
      *) exit 1 ;;
    esac
    ;;
  *) exit 1 ;;
esac
EOF
chmod +x "$temp_root/bin/crane"
export PATH="$temp_root/bin:$PATH"
export FAKE_COPY_LOG="$temp_root/copy.log"

readonly expected_digest='sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef'
output="$(FAKE_TAG_MODE=same bash "$tag_helper" \
  'registry.example/staging/web@sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef' \
  'registry.example/production/web' \
  '1.2.3' \
  "$expected_digest")"
grep -Fq 'no mutation required' <<<"$output"
test ! -e "$FAKE_COPY_LOG"

if FAKE_TAG_MODE=conflict bash "$tag_helper" \
  "registry.example/staging/web@$expected_digest" \
  'registry.example/production/web' \
  '1.2.3' \
  "$expected_digest"; then
  echo "Conflicting production tag must be rejected." >&2
  exit 1
fi
test ! -e "$FAKE_COPY_LOG"

readonly attested_image='registry.example/staging/web@sha256:1111111111111111111111111111111111111111111111111111111111111111'
readonly source_revision='2222222222222222222222222222222222222222'
bash "$attestation_verifier" "$attested_image" "$source_revision" \
  'https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/' \
  'https://github.com/MALIEV-Co-Ltd/Maliev.Web.git'
for invalid_mode in orphan tampered; do
  if FAKE_ATTESTATION_MODE="$invalid_mode" bash "$attestation_verifier" "$attested_image" "$source_revision" \
    'https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/runs/' \
    'https://github.com/MALIEV-Co-Ltd/Maliev.Web.git'; then
    echo "Attestation mode $invalid_mode must be rejected." >&2
    exit 1
  fi
done

cat >"$temp_root/bin/kustomize" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
case "$1" in
  edit)
    case "$2 $3" in
      'set image')
        printf '%s\n' "${4#*=}" >"$FAKE_KUSTOMIZE_STATE"
        ;;
      'remove patch')
        python3 - "$PWD/kustomization.yaml" <<'PY'
from pathlib import Path
import sys
path = Path(sys.argv[1])
lines = [line for line in path.read_text().splitlines() if "build-metadata-patch.yaml" not in line]
path.write_text("\n".join(lines) + "\n")
PY
        ;;
      'add patch')
        printf '  - path: build-metadata-patch.yaml\n' >>"$PWD/kustomization.yaml"
        ;;
      *) exit 1 ;;
    esac
    ;;
  build)
    image="$(cat "$FAKE_KUSTOMIZE_STATE")"
    digest="$(awk '/value:/ {gsub(/"/, "", $2); print $2}' "$PWD/build-metadata-patch.yaml")"
    cat <<YAML
apiVersion: apps/v1
kind: Deployment
metadata:
  name: maliev-web
spec:
  template:
    spec:
      containers:
        - name: maliev-web
          image: $image
          env:
            - name: BuildMetadata__ImageDigest
              value: "$digest"
YAML
    ;;
  *) exit 1 ;;
esac
EOF
chmod +x "$temp_root/bin/kustomize"
export FAKE_KUSTOMIZE_STATE="$temp_root/kustomize-image"

readonly gitops_root="$temp_root/gitops"
readonly production_overlay="$gitops_root/3-apps/maliev-web/overlays/production"
mkdir -p "$production_overlay"
cat >"$production_overlay/kustomization.yaml" <<'EOF'
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
resources:
  - ../../base
patches:
  - path: health-patch.yaml
EOF
git -C "$gitops_root" init -q
git -C "$gitops_root" config user.email test@example.com
git -C "$gitops_root" config user.name Test
git -C "$gitops_root" add .
git -C "$gitops_root" commit -qm fixture

readonly production_image='asia-southeast1-docker.pkg.dev/maliev-website/maliev-web-artifact-prod/maliev-web'
bash "$gitops_updater" "$gitops_root" production "$production_image" "$expected_digest"
grep -Fq "value: \"$expected_digest\"" "$production_overlay/build-metadata-patch.yaml"
mapfile -t changed_paths < <(git -C "$gitops_root" status --porcelain --untracked-files=all | sed 's/^...//' | sort)
test "${#changed_paths[@]}" -eq 2
test "${changed_paths[0]}" = '3-apps/maliev-web/overlays/production/build-metadata-patch.yaml'
test "${changed_paths[1]}" = '3-apps/maliev-web/overlays/production/kustomization.yaml'

if bash "$gitops_updater" "$gitops_root" qa "$production_image" "$expected_digest"; then
  echo "Unsupported GitOps environment must be rejected." >&2
  exit 1
fi

touch "$gitops_root/outside-production-scope.txt"
if bash "$gitops_updater" "$gitops_root" production "$production_image" "$expected_digest"; then
  echo "Cross-scope GitOps mutation must be rejected." >&2
  exit 1
fi

echo "Production promotion control fixture passed."
