---
name: fastendpoints
description: FastEndpoints patterns for Faber — Endpoint, Group, SubGroup, Validator, ICommand/ICommandHandler, TypedResults, ErrorOr integration, structured logging. Use before creating any new Endpoint, Group, or Validator.
---

# FastEndpoints — Faber Patterns

FastEndpoints replaces Controllers and MinimalAPI in Faber. Most features follow the **Request → Command → Handler → Response** pipeline.

Canonical sources to mirror before generating new code:
- `src/api/Modules/Auth/Faber.Modules.Auth.Application/Features/SignIn/`
- `src/api/Modules/Resumes/Faber.Modules.Resumes.Application/Features/Educations/DeleteEducation/`
- `src/api/Modules/Auth/Faber.Modules.Auth.Application/Features/Refresh/`
- `src/api/Modules/Auth/Faber.Modules.Auth.Application/Groups/`
- `src/api/Modules/Resumes/Faber.Modules.Resumes.Application/Groups/`

---

## 1. Full Feature Pipeline

### Request (DTO)

```csharp
// {Feature}Request.cs
public record SignInRequest(string Username, string Password);
```

### Command

```csharp
// {Feature}Command.cs — ICommand<ErrorOr<T>> from FastEndpoints
public record SignInCommand(string Username, string Password)
    : ICommand<ErrorOr<SignInResponse>>;
```

### Response

```csharp
// {Feature}Response.cs
public record SignInResponse(string AccessToken, string RefreshToken);
```

### Validator

```csharp
// {Feature}Validator.cs — Validator<TRequest> from FastEndpoints (FluentValidation-based)
public class SignInValidator : Validator<SignInRequest>
{
    public SignInValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}

// With injected dependencies (async validation)
public class SignUpValidator : Validator<SignUpRequest>
{
    public SignUpValidator(IIdentityModuleApi identityModuleApi)
    {
        RuleFor(x => x.Username)
            .MustAsync(async (username, ct) =>
                await identityModuleApi.IsUsernameUniqueAsync(username, ct))
            .WithMessage("Username already exists");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email")
            .MustAsync(async (email, ct) =>
                await identityModuleApi.IsEmailUniqueAsync(email, ct))
            .WithMessage("Email already exists");

        RuleFor(x => x.Password)
            .Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match");
    }
}
```

Validators are **auto-discovered** by FastEndpoints — no manual registration.

### CommandHandler

```csharp
// {Feature}CommandHandler.cs — ICommandHandler<TCommand, TResponse>
public class SignInCommandHandler(
    IIdentityModuleApi identityModuleApi,
    ILogger<SignInCommandHandler> logger)
    : ICommandHandler<SignInCommand, ErrorOr<SignInResponse>>
{
    private const string HandlerName = nameof(SignInCommandHandler);

    public async Task<ErrorOr<SignInResponse>> ExecuteAsync(SignInCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {Username}", HandlerName, command.Username);

        logger.LogInformation("[STEP] Signing in {Username}", command.Username);
        var token = await identityModuleApi.SignInAsync(command.Username, command.Password, ct);

        if (string.IsNullOrEmpty(token.AccessToken) || string.IsNullOrEmpty(token.RefreshToken))
        {
            logger.LogWarning("[FAIL] {HandlerName} | Sign in failed for {Username}", HandlerName, command.Username);
            return Error.Unauthorized("Auth.SignIn", "Sign in failed");
        }

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | User {Username} signed in successfully",
            HandlerName,
            command.Username);

        return token.MapToResponse();
    }
}
```

**Structured logging convention:**
- `[START]` — handler begins
- `[STEP]` — significant intermediate step
- `[FAIL]` — business logic failure (Warning level)
- `[SUCCESS]` — happy path completed

### Mapper

```csharp
// {Feature}Mapper.cs — static class, extension methods only
public static class SignInMapper
{
    public static SignInCommand MapToCommand(this SignInRequest request)
        => new(request.Username, request.Password);

    public static SignInResponse MapToResponse(this TokenResponse token)
        => new(token.AccessToken, token.RefreshToken);
}
```

### Endpoint

```csharp
// {Feature}Endpoint.cs — Endpoint<TRequest, TResponse>
public class SignInEndpoint(ILogger<SignInEndpoint> logger)
    : Endpoint<SignInRequest, Results<Ok<SignInResponse>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Post("sign-in");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
    }

    public override async Task<Results<Ok<SignInResponse>, UnauthorizedHttpResult>> ExecuteAsync(
        SignInRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP POST] {Path} started for {Username}", path, request.Username);

        var response = await request.MapToCommand().ExecuteAsync(ct);

        if (response.IsError)
        {
            logger.LogWarning(
                "[HTTP POST] {Path} failed for {Username}: {Error}",
                path,
                request.Username,
                response.FirstError.Description);

            return TypedResults.Unauthorized();
        }

        logger.LogInformation(
            "[HTTP POST] {Path} completed successfully for {Username}",
            path,
            request.Username);

        HttpContext.SetCookiesRefreshToken(response.Value.RefreshToken);

        return TypedResults.Ok(response.Value);
    }
}
```

### What the framework returns automatically

- `Validator<TRequest>` failures return **400 Bad Request** before the endpoint handler continues
- `Policies("ResumeOwnerPolicy")` and default auth can return **403 Forbidden** before typed endpoint results run
- These framework/middleware responses usually **do not appear** in the endpoint's `Results<...>` generic signature

---

## 2. Groups and SubGroups

Groups define route prefixes and Swagger tags.

```csharp
// Top-level group → /resumes
public sealed class ResumesGroup : Group
{
    public ResumesGroup()
    {
        Configure("resumes", ep => ep.Tags("Resumes"));
    }
}

// Sub-group → /resumes/{ResumeId}/educations
public sealed class EducationsSubGroup : SubGroup<ResumesGroup>
{
    public EducationsSubGroup()
    {
        Configure("{ResumeId}/educations", ep => ep.Tags("Educations"));
    }
}

// Endpoint uses the sub-group
public class DeleteEducationEndpoint(ILogger<DeleteEducationEndpoint> logger)
    : Endpoint<DeleteEducationRequest, Results<NoContent, NotFound>>
{
    public override void Configure()
    {
        Delete("{Id}");
        Group<EducationsSubGroup>();      // results in DELETE /api/v1/resumes/{ResumeId}/educations/{Id}
        Policies("ResumeOwnerPolicy");
        Version(1);
    }
}
```

Notes:
- `Group<TGroup>()` is the normal pattern for all modules
- `SubGroup<TParent>()` is currently used heavily in `Resumes` for nested resources such as `Educations`, `Courses`, `Skills`, `Links`, and `EmploymentHistories`
- Parent route parameters are part of the subgroup path, so request DTOs typically carry both parent and child ids (for example `ResumeId` + `Id`)

---

## 3. TypedResults Reference

| HTTP Status | TypedResults call |
|---|---|
| 200 OK + body | `TypedResults.Ok(value)` |
| 201 Created | `TypedResults.Created(location, value)` |
| 204 No Content | `TypedResults.NoContent()` |
| 400 Bad Request (manual business error) | `TypedResults.BadRequest(error)` |
| 401 Unauthorized | `TypedResults.Unauthorized()` |
| 403 Forbidden | `TypedResults.Forbid()` |
| 404 Not Found | `TypedResults.NotFound()` |
| 409 Conflict | `TypedResults.Conflict(error)` |

**Multi-response signature:**
```csharp
Results<Ok<TResponse>, NotFound>                        // 2 possible outcomes
Results<NoContent, BadRequest<Error>, UnauthorizedHttpResult>  // 3 possible outcomes
```

Important distinction:
- Validation failures in Faber tests are often asserted as `ErrorResponse` from FastEndpoints, not `BadRequest<Error>` from manual `TypedResults.BadRequest(...)`
- Middleware-produced `403 Forbidden` can exist even when `ForbidHttpResult` is not part of the endpoint response union

---

## 4. Endpoint Variants

### No Request Body

```csharp
public class MeEndpoint(ILogger<MeEndpoint> logger)
    : EndpointWithoutRequest<Results<Ok<MeResponse>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Get("me");
        Group<AuthGroup>();
        Version(1);
    }

    public override async Task<Results<Ok<MeResponse>, UnauthorizedHttpResult>> ExecuteAsync(
        CancellationToken ct)
    {
        var userId = HttpContext.User.FindFirst(JwtClaimTypes.Aliases.UserId)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.Unauthorized();
        }

        var result = await new MeCommand(userId).ExecuteAsync(ct);

        if (result.IsError)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result.Value);
    }
}
```

### Refresh Token (custom base)

```csharp
// Custom base class handles cookie (web) vs header (mobile) extraction
public class RefreshEndpoint(ILogger<RefreshEndpoint> logger)
    : EndpointWithRefreshToken<Results<Ok<RefreshResponse>, BadRequest<Error>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Post("refresh");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
    }

    public override async Task<Results<Ok<RefreshResponse>, BadRequest<Error>, UnauthorizedHttpResult>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken ct)
    {
        var refreshToken = GetRefreshToken(request);

        if (string.IsNullOrEmpty(refreshToken))
        {
            return TypedResults.Unauthorized();
        }

        var response = await request.MapToCommand(refreshToken).ExecuteAsync(ct);

        if (response.IsError)
        {
            return TypedResults.BadRequest(response.FirstError);
        }

        HttpContext.SetCookiesRefreshToken(response.Value.RefreshToken);

        return TypedResults.Ok(response.Value);
    }
}
```

`EndpointWithRefreshToken<TResponse>` in Faber is a real custom base class:
- accepts `RefreshTokenRequest`
- reads refresh token from cookie for web clients
- reads refresh token from request body for mobile clients

---

## 5. HttpContext Utilities

```csharp
// Set refresh token in cookie (web clients)
HttpContext.SetCookiesRefreshToken(refreshToken);

// Extract user id from JWT
var userId = HttpContext.User.FindFirst(JwtClaimTypes.Aliases.UserId)?.Value;

// Extract route parameter
var resumeId = HttpContext.GetRouteValue("ResumeId")?.ToString();
```

In practice:
- `SetCookiesRefreshToken(...)` is used by `SignInEndpoint` and `RefreshEndpoint`
- `FindFirst(JwtClaimTypes.Aliases.UserId)` is common in authenticated endpoints and authorization handlers
- `GetRouteValue("ResumeId")` is used notably inside resume-ownership authorization, not only in endpoints

---

## 6. Command Execution

FastEndpoints commands are executed with the `.ExecuteAsync(ct)` extension:

```csharp
// In endpoint — preferred pattern
var result = await request.MapToCommand().ExecuteAsync(ct);

// Without request DTO mapping
var result = await new MeCommand(userId).ExecuteAsync(ct);
```

This is equivalent to resolving the handler from DI and calling it directly.

---

## 7. Endpoint Configuration Options

| Method | Purpose |
|---|---|
| `Post("route")` / `Get(...)` / `Put(...)` / `Delete(...)` / `Patch(...)` | HTTP verb + route segment |
| `Group<TGroup>()` | Attach to a Group/SubGroup |
| `Version(1)` | API version prefix (`/api/v1/...`) |
| `AllowAnonymous()` | Skip JWT auth |
| `Policies("PolicyName")` | Require authorization policy |
| `Tags("TagName")` | Swagger tag (usually set on Group) |

---

## 8. Testing Endpoints

Use FastEndpoints.Testing helpers:

```csharp
// POST with typed request/response
var (httpRes, response) = await app.Client
    .POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(
        new SignInRequest("user", "pass"));

httpRes.StatusCode.ShouldBe(HttpStatusCode.OK);
response.AccessToken.ShouldNotBeNullOrEmpty();

// POST validation failure — FastEndpoints ErrorResponse
var (badHttpRes, error) = await app.Client
    .POSTAsync<SignInEndpoint, SignInRequest, ErrorResponse>(
        new SignInRequest("", ""));

badHttpRes.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
error.Errors.ShouldContainKey("username");

// DELETE — use the typed route/request DTO, not an anonymous object
var httpRes = await app.Client
    .WithAuthToken(accessToken)
    .DELETEAsync<DeleteEducationEndpoint, DeleteEducationRequest>(
        new DeleteEducationRequest(resumeId, educationId));

httpRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);
```

Real test patterns in Faber:
- use `ErrorResponse` for validator-driven `400` assertions
- use `WithAuthToken(accessToken)` for protected endpoints
- expect `403 Forbidden` for auth/policy failures even if the endpoint's generic return type only lists success/business-error results

---

## DO / DON'T

| DO | DON'T |
|---|---|
| `Validator<TRequest>` for all validation | Manual validation in handler |
| Static `Mapper` extension methods | AutoMapper or inline mapping |
| `ICommand<ErrorOr<T>>` + `ICommandHandler<,>` | Services with multiple methods |
| `TypedResults.*` for type-safe responses | `Results.Ok(...)` untyped |
| `[START]`/`[STEP]`/`[FAIL]`/`[SUCCESS]` log pattern | Free-form log messages |
| Groups/SubGroups for route organisation | Hard-coding full routes in endpoints |
| Let FastEndpoints/auth middleware produce validation and policy errors | Adding fake `BadRequest`/`Forbidden` results just to mirror framework behavior |
