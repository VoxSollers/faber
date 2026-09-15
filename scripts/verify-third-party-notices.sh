#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"
cd "$repo_root"

for required_command in awk comm jq sort; do
  command -v "$required_command" >/dev/null 2>&1 || {
    echo "error: required command is unavailable: $required_command" >&2
    exit 1
  }
done

for required_path in skills-lock.json THIRD_PARTY_NOTICES.md .claude/skills; do
  [[ -e "$required_path" ]] || {
    echo "error: required path is missing: $required_path" >&2
    exit 1
  }
done

temporary_directory="$(mktemp -d)"
trap 'rm -rf "$temporary_directory"' EXIT

has_errors=false

while IFS=$'\t' read -r skill_name source_type source; do
  if [[ "$source_type" != "github" ]]; then
    echo "error: unsupported sourceType '$source_type' for skill '$skill_name' (source '$source'); expected 'github'" >&2
    has_errors=true
  fi
done < <(jq -r '.skills | to_entries[] | [.key, (.value.sourceType // ""), (.value.source // "")] | @tsv' skills-lock.json)

jq -r '.skills[].source | ascii_downcase' skills-lock.json | sort -u > "$temporary_directory/lock-sources"

awk '
  /^## Bundled agent skills$/ { in_bundled_skills = 1; next }
  in_bundled_skills && /^## / { exit }
  in_bundled_skills && /^\| \[/ {
    source = $0
    sub(/^\| \[/, "", source)
    sub(/\]\(.*/, "", source)
    print tolower(source)
  }
' THIRD_PARTY_NOTICES.md | sort -u > "$temporary_directory/notice-sources"

if [[ ! -s "$temporary_directory/notice-sources" ]]; then
  echo "error: no upstream source links found in the Bundled agent skills table" >&2
  has_errors=true
fi

while IFS= read -r source; do
  echo "error: source is in skills-lock.json but missing from THIRD_PARTY_NOTICES.md: $source" >&2
  has_errors=true
done < <(comm -23 "$temporary_directory/lock-sources" "$temporary_directory/notice-sources")

while IFS= read -r source; do
  echo "error: source is in THIRD_PARTY_NOTICES.md but missing from skills-lock.json: $source" >&2
  has_errors=true
done < <(comm -13 "$temporary_directory/lock-sources" "$temporary_directory/notice-sources")

while IFS= read -r -d '' path; do
  if [[ ! -L "$path" ]]; then
    echo "error: skill alias is not a symbolic link: $path" >&2
    has_errors=true
  fi
done < <(find .claude/skills -mindepth 1 -print0)

if [[ "$has_errors" == true ]]; then
  exit 1
fi

echo "Third-party notices verified: sources match skills-lock.json and all skill aliases are symbolic links."
