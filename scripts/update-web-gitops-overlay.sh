#!/usr/bin/env bash
set -euo pipefail

if [[ "$#" -ne 4 ]]; then
  echo "Usage: $0 <gitops-repository> <development|staging|production> <target-image> <sha256-digest>" >&2
  exit 2
fi

readonly gitops_repository="$1"
readonly environment="$2"
readonly target_image="$3"
readonly image_digest="$4"
readonly base_image="asia-southeast1-docker.pkg.dev/maliev-website/maliev-website-artifact/maliev-web"

case "$environment" in
  development|staging|production) ;;
  *)
    echo "Refusing unsupported Web environment: $environment" >&2
    exit 2
    ;;
esac

if [[ ! "$image_digest" =~ ^sha256:[0-9a-f]{64}$ ]]; then
  echo "Refusing invalid image digest: $image_digest" >&2
  exit 2
fi

if [[ ! "$target_image" =~ ^[a-z0-9.-]+-docker\.pkg\.dev/[a-z0-9._/-]+/maliev-web$ ]]; then
  echo "Refusing invalid Web target image: $target_image" >&2
  exit 2
fi

readonly repository_root="$(cd "$gitops_repository" && pwd -P)"
readonly overlay_relative="3-apps/maliev-web/overlays/${environment}"
readonly overlay_path="${repository_root}/${overlay_relative}"
readonly kustomization_path="${overlay_path}/kustomization.yaml"
readonly metadata_patch_path="${overlay_path}/build-metadata-patch.yaml"

if [[ ! -d "$repository_root/.git" ]] && ! git -C "$repository_root" rev-parse --git-dir >/dev/null 2>&1; then
  echo "Refusing non-Git GitOps repository: $repository_root" >&2
  exit 2
fi

if [[ ! -f "$kustomization_path" ]]; then
  echo "Refusing missing Web overlay: $overlay_relative" >&2
  exit 2
fi

(
  cd "$overlay_path"
  kustomize edit set image "${base_image}=${target_image}@${image_digest}"
)

cat >"$metadata_patch_path" <<EOF
apiVersion: apps/v1
kind: Deployment
metadata:
  name: maliev-web
spec:
  template:
    spec:
      containers:
        - name: maliev-web
          env:
            - name: BuildMetadata__ImageDigest
              value: "${image_digest}"
EOF

if ! grep -Eq '^patches:[[:space:]]*$' "$kustomization_path"; then
  echo "Refusing Web overlay without a patches list: $overlay_relative" >&2
  exit 1
fi

sed -i '/^[[:space:]]*-[[:space:]]*path:[[:space:]]*build-metadata-patch\.yaml[[:space:]]*$/d' "$kustomization_path"
readonly patch_indent="$(sed -n 's/^\([[:space:]]*\)-[[:space:]]*path:.*/\1/p' "$kustomization_path" | head -n 1)"
printf '%s- path: build-metadata-patch.yaml\n' "$patch_indent" >>"$kustomization_path"

readonly patch_reference_count="$(grep -Ec '^[[:space:]]*-[[:space:]]*path:[[:space:]]*build-metadata-patch\.yaml[[:space:]]*$' "$kustomization_path")"
if [[ "$patch_reference_count" -ne 1 ]]; then
  echo "Expected build-metadata-patch.yaml to be referenced exactly once." >&2
  exit 1
fi

readonly rendered_manifest="$(mktemp)"
trap 'rm -f -- "$rendered_manifest"' EXIT
(
  cd "$overlay_path"
  kustomize build .
) >"$rendered_manifest"

mapfile -t rendered_images < <(grep -E '^[[:space:]]*image:[[:space:]]*.+/maliev-web(@|:)' "$rendered_manifest" | sed -E 's/^[[:space:]]*image:[[:space:]]*//')
if [[ "${#rendered_images[@]}" -ne 1 ]] || [[ "${rendered_images[0]}" != "${target_image}@${image_digest}" ]]; then
  echo "Web Deployment rendered image digest does not match ${target_image}@${image_digest}." >&2
  exit 1
fi

mapfile -t rendered_metadata_digests < <(
  awk '
    /^[[:space:]]*- name: BuildMetadata__ImageDigest[[:space:]]*$/ {
      if (getline > 0 && $0 ~ /^[[:space:]]*value:/) {
        sub(/^[[:space:]]*value:[[:space:]]*/, "")
        gsub(/^"|"$/, "")
        print
      }
    }
  ' "$rendered_manifest"
)
if [[ "${#rendered_metadata_digests[@]}" -ne 1 ]] || [[ "${rendered_metadata_digests[0]}" != "$image_digest" ]]; then
  echo "Web Deployment rendered BuildMetadata__ImageDigest does not match ${image_digest}." >&2
  exit 1
fi

while IFS= read -r status_line; do
  [[ -z "$status_line" ]] && continue
  changed_path="${status_line:3}"
  case "$changed_path" in
    "${overlay_relative}/kustomization.yaml"|"${overlay_relative}/build-metadata-patch.yaml") ;;
    *)
      echo "Refusing change outside ${overlay_relative}: ${changed_path}" >&2
      exit 1
      ;;
  esac
done < <(git -C "$repository_root" status --porcelain --untracked-files=all)

printf 'Verified Web GitOps image and BuildMetadata digest parity for %s at %s.\n' "$environment" "$image_digest"
