# Plan template — Tier 2 and 3

Save to `docs/plans/{YYYY-MM-DD}-{issue-number}-{kebab-slug}.md`. The issue number is mandatory and always sits between the date and the slug — `docs/plans/` is gitignored, so the filename is the only link back to the issue.

**The plan is a pointer sheet, not the implementation.** It names *where* work happens and *how it is verified*; the implementer reads the actual code. No inline code blocks except the exact shell commands for verify and commit. Never paste a file's future contents into the plan.

A Tier 2 plan stays under ~6 KB. If it grows past that, the issue is a Tier 3.

Fill every placeholder — one left as-is is a plan failure.

---

```markdown
# Issue #{N} — {issue title, verbatim from GitHub}

**Goal:** [one sentence]

**Approach:** [2–3 sentences — the shape of the change, not its code]

## Global constraints

- Branch: `{issue-number}-{kebab-case-description}`
- Build/test command every task verifies against: [exact command, incl. required env vars]
- [naming / module-boundary / testing rules from CLAUDE.md this issue actually touches]
- Conventional Commits; no `Co-Authored-By`.

## Checkpoints

- [ ] Task 1: [name]
- [ ] Task 2: [name]
- [ ] Manual/visual QA (below, if the issue has a user-facing surface)

---

### Task 1: [name]

[One or two sentences: which problem from the issue this task closes.]

**Files**
- Create: `exact/path`
- Modify: `exact/path` — [what changes, in words]
- Delete: `exact/path`
- Test: `exact/path`

**Required skills:** [names that genuinely exist and apply, or `None`]

**Interfaces**
- Consumes: [exact names/signatures produced by earlier tasks]
- Produces: [exact names/signatures later tasks rely on — an implementer sees only its own brief]

**Verify:** `[exact command]` → expects `[exact expected output]`

**Commit:** `type(scope): description`

---

## Manual/visual QA

- [ ] [what to look at] — [exact URL/route/interaction]

[Write "not applicable — [reason]" when the issue has no user-facing surface.]
```

---

## Bookkeeping during execution

Tick a task's checkbox only after its diff is reviewed clean, and append the commit hash on the same line:

```
- [x] Task 1: rename the token — `a1b2c3d`, review clean
```

The checkboxes are the source of truth for what is done — the session's memory of the run is not.
