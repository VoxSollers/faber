#!/usr/bin/env bash

set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
solution_file="$repository_root/Faber.sln"
solutionx_file="$repository_root/Faber.slnx"

for file in "$solution_file" "$solutionx_file"; do
  if [[ ! -f "$file" ]]; then
    printf 'Required solution file not found: %s\n' "$file" >&2
    exit 1
  fi
done

normalize_project_paths() {
  tr '\\' '/' | sed 's#^\./##' | sort -u
}

solution_projects="$({
  sed -nE 's/^Project\([^)]*\) = "[^"]*", "([^"]+\.csproj)", ".*$/\1/p' "$solution_file"
} | normalize_project_paths)"

solutionx_projects="$({
  sed -nE 's/^[[:space:]]*<Project[[:space:]]+Path="([^"]+\.csproj)"[[:space:]]*\/>[[:space:]]*$/\1/p' "$solutionx_file"
} | normalize_project_paths)"

if [[ -z "$solution_projects" || -z "$solutionx_projects" ]]; then
  printf 'Could not extract project paths from both solution files.\n' >&2
  exit 1
fi

missing_from_solution="$(comm -13 <(printf '%s\n' "$solution_projects") <(printf '%s\n' "$solutionx_projects"))"
missing_from_solutionx="$(comm -23 <(printf '%s\n' "$solution_projects") <(printf '%s\n' "$solutionx_projects"))"

if [[ -n "$missing_from_solution" || -n "$missing_from_solutionx" ]]; then
  printf 'Project references differ between Faber.sln and Faber.slnx.\n' >&2

  if [[ -n "$missing_from_solution" ]]; then
    printf 'Missing from Faber.sln:\n' >&2
    while IFS= read -r project; do
      printf '  %s\n' "$project" >&2
    done <<< "$missing_from_solution"
  fi

  if [[ -n "$missing_from_solutionx" ]]; then
    printf 'Missing from Faber.slnx:\n' >&2
    while IFS= read -r project; do
      printf '  %s\n' "$project" >&2
    done <<< "$missing_from_solutionx"
  fi

  exit 1
fi

printf 'Faber.sln and Faber.slnx reference the same project paths.\n'
