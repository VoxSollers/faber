# Refactor Candidates

After TDD cycle, look for:

- **Duplication** → Extract helper method in test class or shared `Data/` class
- **Long command handlers** → Break into private methods (keep tests on HTTP endpoints)
- **Shallow modules** → Combine or deepen (e.g., merge trivial mapper into handler)
- **Feature envy** → Move logic to domain entity where data lives
- **Primitive obsession** → Introduce value objects or strongly-typed IDs
- **Missing guard clauses** → Replace `if-else` chains with early returns
- **Existing code** the new code reveals as problematic
- **Static mapper** duplication → Extract shared extension methods