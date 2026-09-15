# Tier 3 — full orchestration loop

Tier 3 is Tier 2 plus a dedicated reviewer per task and a fix loop. Read this only when triage selected Tier 3 (multiple `area:*` labels, 5+ tasks, a new module or an EF migration).

## Before the plan

- Check the session's effort. If it reads `low` or `medium`, recommend `/effort high` before writing the plan — plan quality and diff review are where deliberation pays.
- If the session is on a small model, offer `/model opus` once. Never nudge the other way.
- Load `superpowers:brainstorming` **only** when the issue is genuinely ambiguous about scope — an issue that names its own acceptance criteria is not ambiguous. Ambiguity means: two defensible readings of what "done" is, and the issue settles neither.

Then follow the Tier 2 plan flow (`references/plan-template.md`, `AskUserQuestion` approval gate). The plan may exceed 6 KB here; it still carries no inline code blocks.

## Per-task loop

For each task, in plan order:

1. **Re-check the task's "Required skills"** against the currently available skills list — a plan written an hour ago can already be stale. Correct the brief, never invent a name.
2. **Dispatch `faber-implementer`** (foreground) with that task's brief only — not the whole plan file.
3. **Dispatch `faber-reviewer`** (foreground) against the resulting diff. Give it: the task brief, the commit range, and the project skills it should judge by. It returns findings plus `CLEAN` or `BLOCKED`.
4. **Fix loop** — on Critical or Important findings, dispatch `faber-implementer` again with the findings as its brief, then re-review with a fresh `faber-reviewer`. Repeat until `CLEAN`. Never move on with an open Critical or Important finding, and never fix the findings yourself — the session reviews at Tier 2 precisely because Tier 3 delegates it.
5. **Two rounds and still blocked** on the same finding means the plan is wrong, not the implementer. Stop, say so, and re-plan that task with the user.
6. **Tick the checkbox** in the plan file with the commit hash and `review clean`, then proceed.

## After the last task

- Run the `code-review` skill on the whole branch.
- Report: tasks completed, commits per task, final review outcome, and every item still unchecked (manual/visual QA especially).
- **Stop there.** No push, no PR, no `finishing-a-development-branch` without explicit user confirmation. If the run was in a worktree, name the worktree path and branch, state the commits are unpushed, and offer the push without performing it.
