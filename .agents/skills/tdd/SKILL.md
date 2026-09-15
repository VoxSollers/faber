---
name: tdd
description: Test-driven development with red-green-refactor loop. Use when user wants to build features or fix bugs using TDD, mentions "red-green-refactor", wants integration tests, or asks for test-first development.
---

# Test-Driven Development (C# / .NET)

## Philosophy

**Core principle**: Tests should verify behavior through public HTTP endpoints, not implementation details. Code can change entirely; tests shouldn't.

**Good tests** are integration-style: they hit real FastEndpoints through `app.Client`, use real infrastructure (Testcontainers), and verify HTTP responses. They describe _what_ the system does, not _how_ it does it. A good test reads like a specification — "registered user with valid credentials should return OK" tells you exactly what capability exists. These tests survive refactors because they don't care about internal structure.

**Bad tests** are coupled to implementation. They mock internal services, test command handlers directly, or verify through external means (like querying the DbContext instead of using the API). The warning sign: your test breaks when you refactor, but behavior hasn't changed.

See [tests.md](tests.md) for examples and [mocking.md](mocking.md) for mocking guidelines.

## Anti-Pattern: Horizontal Slices

**DO NOT write all tests first, then all implementation.** This is "horizontal slicing" — treating RED as "write all tests" and GREEN as "write all code."

This produces **crap tests**:

- Tests written in bulk test _imagined_ behavior, not _actual_ behavior
- You end up testing the _shape_ of things (record properties, endpoint signatures) rather than user-facing behavior
- Tests become insensitive to real changes — they pass when behavior breaks, fail when behavior is fine
- You outrun your headlights, committing to test structure before understanding the implementation

**Correct approach**: Vertical slices via tracer bullets. One test → one implementation → repeat. Each test responds to what you learned from the previous cycle.

```
WRONG (horizontal):
  RED:   test1, test2, test3, test4, test5
  GREEN: impl1, impl2, impl3, impl4, impl5

RIGHT (vertical):
  RED→GREEN: test1→impl1
  RED→GREEN: test2→impl2
  RED→GREEN: test3→impl3
  ...
```

## Workflow

### 1. Planning

Before writing any code:

- [ ] Confirm with user what endpoint/feature is needed
- [ ] Confirm with user which behaviors to test (prioritize)
- [ ] Identify the module boundary (PublicApi, Application, Domain, Infrastructure)
- [ ] List the behaviors to test (not implementation steps)
- [ ] Get user approval on the plan

Ask: "What should the endpoint look like? Which behaviors are most important to test?"

**You can't test everything.** Confirm with the user exactly which behaviors matter most. Focus testing effort on critical paths and complex logic, not every possible edge case.

### 2. Tracer Bullet

Write ONE test that confirms ONE thing about the system:

```
RED:   Write test for first behavior → test fails (compile error or assertion)
GREEN: Write minimal code to pass → test passes
```

This is your tracer bullet — proves the path works end-to-end (endpoint → command → handler → database → response).

### 3. Incremental Loop

For each remaining behavior:

```
RED:   Write next test → fails
GREEN: Minimal code to pass → passes
```

Rules:

- One test at a time
- Only enough code to pass current test
- Don't anticipate future tests
- Keep tests focused on observable HTTP behavior

### 4. Refactor

After all tests pass, look for [refactor candidates](refactoring.md):

- [ ] Extract duplication in test helpers
- [ ] Simplify mappers and command handlers
- [ ] Apply patterns from CLAUDE.md (ErrorOr, guard clauses, static mappers)
- [ ] Consider what new code reveals about existing code
- [ ] Run tests after each refactor step

**Never refactor while RED.** Get to GREEN first.

## Checklist Per Cycle

```
[ ] Test describes behavior, not implementation
[ ] Test uses HTTP endpoint (app.Client), not internal services
[ ] Test would survive internal refactor
[ ] Code is minimal for this test
[ ] No speculative features added
```

## Running Tests

```bash
# Build test project
dotnet build <test-project>.csproj

# Run all tests
dotnet run --project <test-project>.csproj

# Run specific test
dotnet run --project <test-project>.csproj -- -method "*.TestMethodName"

# Uses DOCKER_HOST when set; otherwise defaults to the standard Docker socket
DOCKER_HOST="${DOCKER_HOST:-unix:///var/run/docker.sock}" \
  dotnet run --project <test-project>.csproj
```
