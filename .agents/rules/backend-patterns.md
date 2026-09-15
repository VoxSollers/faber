---
paths:
  - "src/api/**/*.cs"
  - "tests/**/*.cs"
---

# Backend Patterns — Faber

## Module Feature Structure

Canonical example: `src/api/Modules/Auth/Faber.Modules.Auth.Application/Features/SignIn/`

```text
Features/{Feature}/
├── {Feature}Endpoint.cs        # FastEndpoints endpoint (primary constructor for DI)
├── {Feature}Request.cs         # record
├── {Feature}Response.cs        # record
├── {Feature}Command.cs         # record implementing ICommand<ErrorOr<TResponse>>
├── {Feature}CommandHandler.cs  # extends CommandHandlerWithCatchError
├── {Feature}Mapper.cs          # static class with extension methods
└── {Feature}Validator.cs       # extends Validator<TRequest>
```

**Module boundaries:**
- `{Module}.PublicApi` — `I{Module}ModuleApi` interface (cross-module contract)
- `{Module}.Application` — `{Module}ModuleApi` implementation, features, groups
- Groups: `Groups/{Module}Group.cs` — extends FastEndpoints `Group`

## Naming Conventions

| Concept            | Pattern                                    | Example                                    |
|--------------------|--------------------------------------------|--------------------------------------------|
| Endpoint           | `{Feature}Endpoint`                        | `SignInEndpoint`                           |
| Request / Response | `{Feature}Request` / `{Feature}Response`   | `SignInRequest` / `SignInResponse`         |
| Command            | `{Feature}Command`                         | `SignInCommand`                            |
| Command Handler    | `{Feature}CommandHandler`                  | `SignInCommandHandler`                     |
| Validator          | `{Feature}Validator`                       | `SignInValidator`                          |
| Mapper             | `{Feature}Mapper`                          | `SignInMapper`                             |
| Group              | `{Module}Group`                            | `AuthGroup`                                |
| Module API         | `I{Module}ModuleApi` / `{Module}ModuleApi` | `IIdentityModuleApi` / `IdentityModuleApi` |
| Tests              | `{Feature}Tests`                           | `SignInTests`                              |
| Test data          | `{Purpose}Data` / `{Feature}Constants`     | `RegisteredUsersData` / `SignInConstants`  |
| Options            | `{Name}Options` / `{Name}OptionsSetup`     | `KeycloakOptions` / `KeycloakOptionsSetup` |

## C# Patterns

**Use:** records for DTOs/commands/requests/responses · primary constructors for DI · file-scoped namespaces · `ErrorOr<T>` for results · static mapper classes · guard clauses / early returns · `CancellationToken ct` as last async parameter · Options Pattern with `IOptions<T>` / `IConfigureOptions<T>` · curly braces `{}` for all `if` blocks.

**Avoid:** AutoMapper · repository pattern over EF Core · exceptions for flow control · `#region`/`#endregion` · `if-else` chains.

## Tests

**Test class pattern:** `[Collection<CollectionAuth>] [Priority(N)] public class {Feature}Tests(WebApp app) : TestBase`

**Test method naming:** `{Scenario}_Should{Outcome}` (e.g. `EmptyUsername_ShouldReturnBadRequest`)

**Test file structure:** `Features/{Feature}/{Feature}Tests.cs`, `{Feature}Constants.cs` (imported via `using static`), `Data/{Description}Data.cs` + `{Feature}Faker.cs`

**Guidelines:**
- Testcontainers for all infrastructure — no shared or persistent test databases
- Mock only external services (Vault, email), not internal abstractions
- `ClassData` > `InlineData` — use Bogus for generated data; `InlineData` only for literals
- Data classes implement `IEnumerable<TheoryDataRow<T>>` with seeded fakers for deterministic data
- Register record types used in `TheoryDataRow` via `RecordSerializer` (`[assembly: RegisterXunitSerializer]`)
- Shouldly over FluentAssertions; NSubstitute over Moq
- Keycloak fields (username, email): `ShouldBe(expected, StringCompareShould.IgnoreCase)`
- `CancellationToken` via `TestContext.Current.CancellationToken`

---
Expanded guidance and examples: `dotnet-backend-patterns`, `fastendpoints`, `errorOr-patterns`.
