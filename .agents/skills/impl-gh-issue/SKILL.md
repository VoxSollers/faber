---
name: impl-gh-issue
description: Implement a GitHub issue end-to-end, sized to the issue
argument-hint: <issue-number> [--tier N] [--worktree]
arguments: issue
disable-model-invocation: true
allowed-tools: Bash(gh issue view *) Bash(git *)
---

## Context

- Issue: !`gh issue view $issue --json number,title,body,url,labels,comments 2>&1 || true`
- Branch: !`git branch --show-current 2>&1 || true`
- Dirty files: !`git status --short 2>&1 || true`

The issue number must be the **first** argument. Everything above is data, not truth: if the issue block is an error ("could not resolve to an Issue", auth failure, empty), stop and report it — never guess at issue content.

Flags, any order after the number: `--tier N` (override triage), `--worktree` (run in a worktree). No number, or a non-numeric first argument → stop and ask which issue.

## Triage — first match wins

Evaluate against the injected labels and body only. No code reading, no extra `gh` calls. Count the files a rule asks about by what the change **touches**, not by every path the prose mentions.

| # | Rule | Tier |
|---|---|---|
| 1 | `--tier N` was passed | **N** — the user wins, no argument |
| 2 | ≥2 `area:*` labels, **or** the body names a new module / EF migration / new DbContext | **3** |
| 3 | `type:feat` or `type:refactor` with more than 4 items in the body's Scope / Plan / Acceptance list | **3** |
| 4 | `type:docs` or `type:chore`, **and** the body names ≤2 files **to change** | **1** |
| 5 | `type:fix` with exactly one `area:*`, **and** the body names one file, token, or symbol **to change** | **1** |
| 6 | anything else | **2** — the safe default |

**Announce, don't ask.** State the tier and the rule number that fired in one line, then proceed. Tier 1 has no approval round-trip — that round-trip is most of what Tier 1 saves. Tiers 2/3 gate at the plan, so a wrong tier is corrected there.

| Tier | Pipeline |
|---|---|
| **1 — Direct** | session edits → verify → commit → `code-review` on the diff. No plan file, no subagents. |
| **2 — Light** | compact plan → approval → one `faber-implementer` per task, foreground → **session** reviews each diff → one `code-review` on the branch |
| **3 — Full** | Tier 2 + per-task `faber-reviewer` and fix loop — read `references/tier-3.md` |

## Branch

If on `main`, create and check out `{issue-number}-{kebab-case-description}`. If already on this issue's branch, stay. Never start work on `main`.

## Tier 1

Edit directly, run the issue's verify command, commit (Conventional Commits, no `Co-Authored-By`), then run the `code-review` skill on the diff.

**Escape hatch — stop and re-propose Tier 2** when any of these trips:

- the edit would touch more than 2 files
- the verify command fails twice in a row
- the fix needs a design decision the issue does not settle

## Tier 2

1. Read `references/plan-template.md`. Write the plan to `docs/plans/{YYYY-MM-DD}-{issue-number}-{kebab-slug}.md` — gitignored, so the filename is the only link back to the issue. Checkpoints, files, interfaces, verify command. **No inline code blocks**; a Tier 2 plan stays under ~6 KB.
2. Compose each task brief with `references/skill-routing.md` for the `area:*` → project-skill map. Never invent a skill name — write `None`.
3. Get explicit approval via `AskUserQuestion` before executing. An ambiguous answer is not approval. Revise the plan file in place and re-present if changes are requested.
4. Dispatch per task, then review the diff **yourself** — no reviewer subagent at this tier. Tick the task's checkbox with its commit hash before moving on.
5. After the last task, run `code-review` on the branch.

## Dispatch rules (all tiers)

- One task per dispatch — hand the implementer its task brief, not the whole plan file.
- Foreground only (`run_in_background: false`), in strict order. Each task depends on the last.
- Check the result before moving on. Never fire-and-forget.
- Never proceed past a failed verify.

## Model and effort

This skill sets **no `model:` and no `effort:` override** — frontmatter freezes before the tier is known, so any static value is wrong half the time.

- The session runs on whatever model the user is already on.
- Implementers and reviewers are pinned `sonnet` in `.claude/agents/`, so the session's model never multiplies by task count. Escalate a single unusually hard task by passing `model` on that one dispatch.
- One nudge, one direction: if triage lands **Tier 3** on a small model, say so and offer `/model opus`. Never nudge the other way.
- Current session effort: `${CLAUDE_EFFORT}`. If the tier is 3 and this reads `low` or `medium`, recommend `/effort high` **before** writing the plan.

## Worktree (`--worktree`)

Available at every tier and composes with `--tier N`. The skill **never** opens a worktree on its own judgement — only on this flag.

### Branch resolution (GitHub-linked)

Resolve before touching `EnterWorktree`. This is what lets Claude RC's worktree mode discover and resume the right branch for an issue — GitHub's own "linked branches" record, not a name the skill invents locally.

0. **Already inside a worktree session?** Check `git rev-parse --git-dir` — inside a linked worktree it resolves under `.git/worktrees/<name>`, vs. a bare `.git` in the main tree. This is the RC worktree-mode case: RC already created and switched into the worktree before the skill ran, so `EnterWorktree` must never be called here — it errors when called from inside an existing worktree session. Instead:
   - Current branch already linked (appears in `gh issue develop --list <issue-number>`) → nothing to do, skip straight to the tier pipeline.
   - Not linked yet → link the branch already checked out: `gh issue develop <issue-number> --name <current-branch-name>`. Don't rename the branch and don't pre-push — GitHub creates the linked ref from the default branch, still an ancestor of the local branch at this point, so a later push still fast-forwards. If the call errors because a branch of that name already exists remotely, treat that as already-linked and continue.
   - Either way, skip steps 1–3 below — no `EnterWorktree` call in this branch of the logic.
0b. **Already on the issue's branch, but not inside a worktree session?** (main tree, and the current branch starts with `{issue-number}-`.) The user already did the branch work by hand in an ordinary session. Treat as resolved — skip steps 1–3, go straight to the tier pipeline. Don't call `gh issue develop` here — working locally without a linked branch is the user's call, not something to silently change.
1. Compute the candidate branch name: `{issue-number}-{kebab-case-description}` — same rule the plain `## Branch` section uses.
2. Query GitHub's linked branches: `gh issue develop --list <issue-number>` (no `-R` needed — `gh` resolves the repo from cwd).
   - **Exactly one branch listed** → use that name verbatim, even if it differs from the candidate slug. GitHub's record is authoritative, not the guessed slug.
   - **More than one listed** → stop and ask via `AskUserQuestion` which branch to resume. Never guess.
   - **None listed** → create the link: `gh issue develop <issue-number> --name <candidate>` (no `-c`/`--checkout` — the working-tree checkout happens via `EnterWorktree`, not the current session's tree).
3. Check whether a worktree for the resolved branch is already open (`git worktree list`, matching `.claude/worktrees/<name>`).
   - Already open → `EnterWorktree(path: ".claude/worktrees/<name>")` to resume it, instead of `name:`.
   - Not open → `EnterWorktree(name: "<name>")`, using the **full resolved branch name**, not just the issue number — this keeps the local worktree and the GitHub-linked branch from diverging.
4. Resolved name longer than `EnterWorktree`'s 64-character limit → stop and ask the user how to proceed (e.g. a shorter GitHub-linked branch name) rather than silently truncating or falling back to raw `git worktree add`.

### Rules

- Never `git worktree add` — the two mechanisms do not know about each other, and `ExitWorktree` will not clean up raw trees.
- The base ref is `fresh` (`origin/<default-branch>`), not local HEAD. On a dirty tree, say so before creating: uncommitted local changes do not come along.
- **Never call `ExitWorktree`.** Teardown is the user's decision at the session-exit prompt.
- End every worktree run by naming `.claude/worktrees/<resolved-branch-name>` and the branch, stating the commits are **not pushed**, and *offering* the push — never performing it.
- Parallelism is N sessions in N worktrees, not one session juggling N issues.

| Work | Parallel? |
|---|---|
| `dotnet build` / `ng build`, `ng test` | yes |
| Integration tests (Testcontainers — random host ports) | yes, Docker RAM is the limit |
| A running Aspire stack | **no** — ports hard-pinned in `AppHost/Program.cs` |
| Manual / visual QA | **no** — follows from the row above |

A run inside a worktree must not assume the running stack serves *its* tree. That is a wrong-result hazard, not a slow one.

## Hard stops

- **Never push, never open a PR, never run `finishing-a-development-branch`** without explicit user confirmation. Stop and report instead.
- Conventional Commits; no `Co-Authored-By` or other self-references.
- Leave unchecked items (manual/visual QA) visible rather than ticking them on the user's behalf.
- Load `superpowers:brainstorming` only at Tier 3, and only when the issue is genuinely ambiguous about scope.
