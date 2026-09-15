---
name: github-workflow
description: Faber's GitHub issue/PR conventions — required labels, project board, release-please flow, and issue/PR templates. Use when creating or labeling issues, opening PRs, or handling a release.
---

# GitHub Workflow

Every issue must carry exactly one `type:*`, one `priority:*`, and at least one `area:*` label. New issues land in the `Faber` Project's `Inbox` status via `.github/workflows/add-to-project.yml` and are triaged weekly.

**Labels** (orthogonal, mutually exclusive within each axis):
- `type:` — `feat` · `fix` · `refactor` · `chore` · `test` · `docs` · `perf`
- `priority:` — `p0` (blocker) · `p1` (current focus) · `p2` (standard) · `p3` (nice-to-have)
- `area:` — `auth` · `resumes` · `users` · `identity` · `notifications` · `vault` · `infra` · `ui-shared` · `ux` · `tests-infra`

**Project board** (`Faber`, user-level): single Kanban with columns `Inbox → Backlog → Up Next → In Progress → In Review → Done`. Custom fields: `Priority`, `Area`, `Iteration`, `Size`.

**Release flow:** Conventional Commit titles on squash-merged PRs feed [release-please](https://github.com/googleapis/release-please). After merging to `main`, the bot opens (or updates) a release PR with the proposed semver bump and `CHANGELOG.md`. Merge the release PR to ship — this creates the git tag and GitHub Release automatically.
- `feat` → minor bump · `fix`/`perf` → patch · `feat!` / `BREAKING CHANGE:` → major
- PR titles are linted by `amannn/action-semantic-pull-request` on every PR (`.github/workflows/pr-title-lint.yml`)

## Pull request preflight

Before creating or editing a PR, choose a conventional title in the form
`type(scope): description`. `type` must be one of the types configured by the
title-lint workflow, and `scope` is required; use the affected area when one
applies (for example, `feat(resumes): add live accordion titles`). Pass that
exact title to `gh pr create` or `gh pr edit`, then verify the persisted title
with `gh pr view --json title`. Correct it before reporting the PR as ready;
do not rely on CI to find a prose-only title.

**Templates** live in `.github/ISSUE_TEMPLATE/` — bug, feature, refactor, chore. Each form auto-applies the matching `type:*` and a default `priority:*` and asks for the `area:*` via dropdown. PR template requires `Closes #N` so the linked issue auto-closes on merge.
