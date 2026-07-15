#!/usr/bin/env bash
set -euo pipefail

if [[ "$#" -ne 4 ]]; then
  echo "Usage: $0 <source-image@digest> <target-repository> <immutable-tag> <expected-digest>" >&2
  exit 2
fi

readonly source_reference="$1"
readonly target_repository="$2"
readonly target_tag="$3"
readonly expected_digest="$4"
readonly target_reference="${target_repository}:${target_tag}"

if [[ ! "$expected_digest" =~ ^sha256:[0-9a-f]{64}$ ]]; then
  echo "Refusing invalid expected digest: $expected_digest" >&2
  exit 2
fi

if [[ ! "$target_tag" =~ ^(dev-[0-9a-f]{12}|[0-9]+\.[0-9]+\.[0-9]+)$ ]]; then
  echo "Refusing invalid immutable tag: $target_tag" >&2
  exit 2
fi

readonly source_digest="$(crane digest "$source_reference")"
if [[ "$source_digest" != "$expected_digest" ]]; then
  echo "Source ${source_reference} resolves to ${source_digest}, expected ${expected_digest}." >&2
  exit 1
fi

readonly lookup_error="$(mktemp)"
trap 'rm -f -- "$lookup_error"' EXIT
if existing_digest="$(crane digest "$target_reference" 2>"$lookup_error")"; then
  if [[ "$existing_digest" == "$expected_digest" ]]; then
    echo "Immutable tag ${target_reference} already resolves to the requested digest ${expected_digest}; no mutation required."
    exit 0
  fi

  echo "Refusing to overwrite immutable tag ${target_reference}: existing ${existing_digest}, requested ${expected_digest}." >&2
  exit 1
fi

if ! grep -Eiq 'MANIFEST_UNKNOWN|manifest unknown|not found|404' "$lookup_error"; then
  echo "Refusing to create immutable tag because its current state could not be verified:" >&2
  cat "$lookup_error" >&2
  exit 1
fi

crane copy "$source_reference" "$target_reference"
readonly published_digest="$(crane digest "$target_reference")"
if [[ "$published_digest" != "$expected_digest" ]]; then
  echo "Immutable tag readback mismatch for ${target_reference}: ${published_digest}, expected ${expected_digest}." >&2
  exit 1
fi

echo "Created immutable tag ${target_reference} at ${expected_digest}."
