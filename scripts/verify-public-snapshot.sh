#!/usr/bin/env bash

set -euo pipefail

usage() {
  cat <<'EOF'
Usage: scripts/verify-public-snapshot.sh [--preflight | --list-candidates]

Without arguments, verifies the final clean-snapshot repository and fails closed.
Use --preflight in the private repository to validate and scan a tracked-files-only
export without traversing the known private Git history. Both require gitleaks,
trufflehog, and jq; a missing tool is a failed verification.

Use --list-candidates to review the same export locally. It reports every scan
candidate with its detector, location, fingerprint and approval status, so a
reviewed false positive can be recorded in approved_false_positive. It prints
fingerprints, never candidate values, and needs only trufflehog and jq.
EOF
}

mode="final"
case "${1:-}" in
  "") ;;
  --preflight) mode="preflight" ;;
  --list-candidates) mode="list" ;;
  -h|--help) usage; exit 0 ;;
  *) usage >&2; exit 2 ;;
esac

repo_root="$(git rev-parse --show-toplevel 2>/dev/null)" || {
  echo "error: run this script inside a Git repository" >&2
  exit 1
}
cd "$repo_root"

required_files=(
  LICENSE
  README.md
  SECURITY.md
  CONTRIBUTING.md
  CODE_OF_CONDUCT.md
  .github/workflows/dotnet.yml
  .github/workflows/frontend.yml
  .github/workflows/container-build.yml
  .github/workflows/publication-readiness.yml
  scripts/verify-public-snapshot.sh
  scripts/verify-third-party-notices.sh
  src/api/Faber.Api/Dockerfile
  src/api/Jobs/Faber.Migrations/Dockerfile
  src/web/faber-app/Dockerfile
)

if [[ "$mode" == "final" ]]; then
  required_files+=(THIRD_PARTY_NOTICES.md)
fi

for path in "${required_files[@]}"; do
  [[ -f "$path" ]] || {
    echo "error: required public-snapshot file is missing: $path" >&2
    exit 1
  }
  git ls-files --error-unmatch -- "$path" >/dev/null 2>&1 || {
    echo "error: required public-snapshot file is not tracked: $path" >&2
    exit 1
  }
done

# Both tracked-export modes scan `git archive HEAD`, so uncommitted work would
# be reported against a tree that does not match what the author is looking at.
if [[ "$mode" != "final" ]]; then
  git diff --quiet --ignore-submodules -- || {
    echo "error: scanning the tracked export requires no unstaged changes to tracked files" >&2
    exit 1
  }
  git diff --cached --quiet --ignore-submodules -- || {
    echo "error: scanning the tracked export requires no staged changes relative to HEAD" >&2
    exit 1
  }
fi

forbidden_tracked="$(
  git ls-files | awk '
    /(^|\/)\.env($|\.)/ && $0 !~ /(^|\/)\.env\.example$/ { print; next }
    /(^|\/)infra\/vault\/file(\/|$)/ { print; next }
    /(^|\/)infra\/vault\/\.vault-unseal-keys$/ { print; next }
    /(^|\/)infra\/vault\/agent\/(role-id|secret-id|token)$/ { print; next }
    /(^|\/)infra\/vault\/certs(\/|$)/ { print; next }
    /(^|\/)certs(\/|$)/ { print; next }
    /\.(key|pem|crt|p12|pfx|jks|keystore)$/ { print; next }
  '
)"
if [[ -n "$forbidden_tracked" ]]; then
  echo "error: credential-bearing path patterns are tracked:" >&2
  printf '%s\n' "$forbidden_tracked" >&2
  exit 1
fi

if git ls-files --stage | awk '$1 == "160000" { found=1 } END { exit !found }'; then
  echo "error: Git submodules are not allowed in the tracked-only snapshot" >&2
  exit 1
fi

if [[ "$mode" == "final" ]]; then
  [[ -z "$(git status --porcelain=v1 --untracked-files=all)" ]] || {
    echo "error: final verification requires a clean working tree" >&2
    exit 1
  }

  [[ -z "$(git tag --list)" ]] || {
    echo "error: final snapshot must not contain imported tags" >&2
    exit 1
  }

  commit_count="$(
    git cat-file --batch-all-objects --batch-check='%(objecttype)' |
      awk '$1 == "commit" { count++ } END { print count+0 }'
  )"
  [[ "$commit_count" == "1" ]] || {
    echo "error: final repository must contain exactly one commit object; found $commit_count" >&2
    exit 1
  }

  root_count="$(git rev-list --max-parents=0 --all | sort -u | awk 'NF { count++ } END { print count+0 }')"
  [[ "$root_count" == "1" ]] || {
    echo "error: final repository must expose exactly one root commit; found $root_count" >&2
    exit 1
  }

  author_email="$(git show -s --format='%ae' HEAD)"
  [[ "$author_email" == *@users.noreply.github.com ]] || {
    echo "error: initial snapshot author must use a GitHub no-reply address" >&2
    exit 1
  }
fi

scan_root="$(mktemp -d "${TMPDIR:-/tmp}/faber-public-snapshot.XXXXXX")"
cleanup() {
  rm -rf -- "$scan_root"
}
trap cleanup EXIT
mkdir "$scan_root/tree"
git archive --format=tar HEAD | tar -xf - -C "$scan_root/tree"

required_tools=(trufflehog jq)
[[ "$mode" == "list" ]] || required_tools=(gitleaks "${required_tools[@]}")

missing_tools=()
for tool in "${required_tools[@]}"; do
  command -v "$tool" >/dev/null 2>&1 || missing_tools+=("$tool")
done
if (( ${#missing_tools[@]} > 0 )); then
  echo "error: required verification tools are missing: ${missing_tools[*]}" >&2
  exit 1
fi

run_gitleaks() {
  gitleaks_source=(dir "$scan_root/tree")
  [[ "$mode" == "final" ]] && gitleaks_source=(git "$repo_root")
  if ! gitleaks "${gitleaks_source[@]}" --no-banner --redact=100 \
    >"$scan_root/gitleaks.log" 2>&1; then
    echo "error: gitleaks did not approve the tracked snapshot; inspect locally with redaction enabled" >&2
    exit 1
  fi
  echo "ok: gitleaks approved the tracked snapshot"
}

run_trufflehog() {
  # Verification would send candidates to third-party services. Run fully
  # offline and retain JSON only in the private temporary directory. Known
  # benign detector matches are keyed on a fingerprint of the candidate itself,
  # so editing the surrounding file freely keeps the exception while rewriting
  # the candidate retires it and forces a fresh review.
  trufflehog_source=(filesystem "$scan_root/tree")
  [[ "$mode" == "final" ]] && trufflehog_source=(git "file://$repo_root")
  if ! trufflehog "${trufflehog_source[@]}" --no-verification \
    --results=unverified --json --fail-on-scan-errors --no-update \
    >"$scan_root/trufflehog.jsonl" 2>"$scan_root/trufflehog.log"; then
    echo "error: trufflehog could not complete the offline tracked-snapshot scan" >&2
    exit 1
  fi

  # The candidate is carried as base64 because a raw match may itself contain
  # tabs or newlines that would break the tab-separated record.
  : >"$scan_root/trufflehog-findings.tsv"
  if [[ -s "$scan_root/trufflehog.jsonl" ]] && ! jq -r \
    '[.DetectorName, (.SourceMetadata.Data.Filesystem.file // .SourceMetadata.Data.Git.file // ""), ((.SourceMetadata.Data.Filesystem.line // .SourceMetadata.Data.Git.line // 0) | tostring), ((if ((.RawV2 // "") | length) > 0 then .RawV2 else .Raw end) | @base64)] | @tsv' \
    "$scan_root/trufflehog.jsonl" >"$scan_root/trufflehog-findings.tsv"; then
    echo "error: trufflehog returned output that could not be evaluated safely" >&2
    exit 1
  fi

  sha256_stdin() {
    if command -v sha256sum >/dev/null 2>&1; then
      sha256sum | awk '{ print $1 }'
    else
      shasum -a 256 | awk '{ print $1 }'
    fi
  }

  # Fingerprint the base64 form rather than the decoded bytes: base64 --decode
  # is spelled differently on GNU and BSD, and the value only has to be stable,
  # not readable. --list-candidates prints it, so nobody recomputes it by hand.
  #
  # Some detectors report a one- or two-character Raw and put the real match in
  # RawV2, which would otherwise fingerprint almost nothing. Prefer RawV2.
  candidate_fingerprint() {
    printf '%s' "$1" | sha256_stdin
  }

  approved_false_positive() {
    local detector="$1"
    local relative_path="$2"
    local fingerprint="$3"

    # Reviewed false positives, keyed on the candidate rather than its
    # surroundings. Add an entry only after reading the flagged text itself;
    # scripts/verify-public-snapshot.sh --list-candidates prints the fingerprint.
    case "${detector}:${relative_path}:${fingerprint}" in
      # <UserSecretsId> GUID, mistaken for a registry credential.
      Dockerhub:src/api/Faber.Api/Faber.Api.csproj:10b242ec13b3ab9681f9f4c8e59da88f5c717fb7452021b957371afe43a0bab4) ;;
      # Keycloak's KC_DB_URL: host, port and database name, no credentials.
      JDBC:src/api/Aspire/Faber.AppHost/Program.cs:bcd10c3813c6d2a6b6b67df4e4eaa4baf6e61dd520033e666ff4234703f3f531) ;;
      # Placeholder connection string in a documentation example.
      SQLServer:.agents/skills/dotnet-backend-patterns/references/dapper-patterns.md:fe13fafe6b82f23f8cf41d7ef6ed1150860c44e828f0ce6a04f80354de02591d) ;;
      # Deliberately unreachable connection string in a migration test.
      SQLServer:tests/Integration/Faber.Migrations.Tests/MigrationWorkerTests.cs:79527e958dc3d07f116a7a2e2b38c6f661796aa66309ba0868f3608cdc070926) ;;
      *) return 1 ;;
    esac
  }

  unexpected_findings=()
  listed_candidates=()
  while IFS=$'\t' read -r detector file_path line_number candidate_b64; do
    [[ -n "$detector" ]] || continue
    # TruffleHog canonicalizes /private/var to /var on macOS, so normalize at
    # the exported tree boundary instead of comparing absolute temp prefixes.
    relative_path="${file_path#*/tree/}"
    relative_path="${relative_path#./}"
    fingerprint="$(candidate_fingerprint "$candidate_b64")"
    if approved_false_positive "$detector" "$relative_path" "$fingerprint"; then
      status="approved"
    else
      status="UNAPPROVED"
      unexpected_findings+=("${detector}"$'\t'"${relative_path}:${line_number}")
    fi
    listed_candidates+=("${detector}"$'\t'"${relative_path}:${line_number}"$'\t'"${fingerprint}"$'\t'"${status}")
  done <"$scan_root/trufflehog-findings.tsv"

  if [[ "$mode" == "list" ]]; then
    if (( ${#listed_candidates[@]} == 0 )); then
      echo "no scan candidates in the tracked export"
      return 0
    fi
    {
      printf 'DETECTOR\tLOCATION\tFINGERPRINT\tSTATUS\n'
      printf '%s\n' "${listed_candidates[@]}"
    } | { column -t -s $'\t' 2>/dev/null || cat; }
    return 0
  fi

  # Locations only. A fingerprint of a candidate that turns out to be a real
  # secret would let anyone reading a public build log confirm a guess offline,
  # so it stays in --list-candidates, which only ever runs locally.
  (( ${#unexpected_findings[@]} == 0 )) || {
    echo "error: trufflehog found ${#unexpected_findings[@]} unapproved candidate(s):" >&2
    printf '  %s\n' "${unexpected_findings[@]}" >&2
    echo "note: review each locally with scripts/verify-public-snapshot.sh --list-candidates" >&2
    exit 1
  }
  echo "ok: trufflehog approved the tracked snapshot"
}

# Listing reviews trufflehog's own approval list, so gitleaks has nothing to add.
if [[ "$mode" == "list" ]]; then
  run_trufflehog
  exit 0
fi

run_gitleaks
run_trufflehog

if [[ "$mode" == "preflight" ]]; then
  echo "ok: tracked-only preflight passed (private Git history was not scanned)"
else
  echo "ok: final clean-snapshot verification passed"
fi
