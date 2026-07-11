#!/usr/bin/env bash
set -euo pipefail

readonly messaging_source="${1:-.ci-sources/Maliev.MessagingContracts}"
readonly aspire_source="${2:-.ci-sources/Maliev.Aspire}"
readonly output_path="${3:-.ci-packages}"
readonly messaging_commit="d4836f135d1cf311b2a490d9ba03809ff295e854"
readonly aspire_commit="7121d57705fc1eff6c7ebb6a69e33e9c26ebfccc"
readonly messaging_version="1.0.90-alpha"
readonly service_defaults_version="1.0.81-alpha"
readonly nuget_org="https://api.nuget.org/v3/index.json"
readonly ci_nuget_config="$(pwd)/NuGet.PRValidation.Config"

assert_checkout() {
  local repository_path="$1"
  local expected_commit="$2"
  local actual_commit

  actual_commit="$(git -C "$repository_path" rev-parse HEAD)"
  if [[ "$actual_commit" != "$expected_commit" ]]; then
    printf 'Expected %s at %s, found %s\n' "$repository_path" "$expected_commit" "$actual_commit" >&2
    exit 1
  fi
}

assert_checkout "$messaging_source" "$messaging_commit"
assert_checkout "$aspire_source" "$aspire_commit"

mkdir -p -- "$output_path"
find "$output_path" -maxdepth 1 -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -delete
readonly output_dir="$(cd "$output_path" && pwd)"

readonly messaging_project="$messaging_source/generated/csharp/Maliev.MessagingContracts.csproj"
readonly service_defaults_project="$aspire_source/Maliev.Aspire.ServiceDefaults/Maliev.Aspire.ServiceDefaults.csproj"

(cd "$messaging_source" && dotnet run --project tools/Generator/Generator.csproj --configuration Release)
dotnet restore "$messaging_project" --configfile "$ci_nuget_config" --source "$nuget_org"
dotnet pack "$messaging_project" \
  --configuration Release \
  --no-restore \
  -p:NoWarn=CS1570 \
  -p:PackageVersion="$messaging_version" \
  --output "$output_dir"

dotnet restore "$service_defaults_project" \
  --configfile "$ci_nuget_config" \
  --source "$nuget_org" \
  --source "$output_dir" \
  -p:GITHUB_ACTIONS=true \
  -p:SharedLibraryVersion="$messaging_version"
dotnet pack "$service_defaults_project" \
  --configuration Release \
  --no-restore \
  -p:GITHUB_ACTIONS=true \
  -p:SharedLibraryVersion="$messaging_version" \
  -p:PackageVersion="$service_defaults_version" \
  --output "$output_dir"

test -s "$output_dir/Maliev.MessagingContracts.$messaging_version.nupkg"
test -s "$output_dir/Maliev.Aspire.ServiceDefaults.$service_defaults_version.nupkg"
