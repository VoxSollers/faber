# Good and Bad Tests

## Good Tests

**Integration-style**: Test through real HTTP endpoints with Testcontainers infrastructure.

```csharp
// GOOD: Tests observable behavior through HTTP
[Fact]
[Priority(1)]
public async Task ValidCreateResume_ShouldReturnOk()
{
    var accessToken = await SignInAsRegisteredUserAsync();
    var request = new CreateResumeRequest("en-us");

    var (httpResponse, response) = await app.Client
        .WithAuthToken(accessToken)
        .POSTAsync<CreateResumeEndpoint, CreateResumeRequest, CreateResumeResponse>(request);

    httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    response.Id.ShouldNotBe(Guid.Empty);
    response.Localization.ShouldBe("en-us");
}
```

Characteristics:

- Tests behavior users/callers care about (HTTP status, response body)
- Uses `app.Client` and FastEndpoints testing helpers
- Survives internal refactors (handler rewrite, mapper changes)
- Describes WHAT, not HOW
- One logical assertion per test (related assertions on same response are fine)

## Bad Tests

**Implementation-detail tests**: Coupled to internal structure.

```csharp
// BAD: Tests command handler directly, bypassing endpoint
[Fact]
public async Task CreateResume_HandlerShouldCallDbContext()
{
    var mockDb = Substitute.For<ResumesDbContext>();
    var handler = new CreateResumeCommandHandler(mockDb);
    var command = new CreateResumeCommand("en-us", userId);

    await handler.ExecuteAsync(command, ct);

    await mockDb.Resumes.Received(1).AddAsync(Arg.Any<Resume>(), ct);
}
```

Red flags:

- Mocking DbContext or internal services
- Testing command handlers directly instead of through endpoints
- Asserting on call counts/order with NSubstitute
- Test breaks when refactoring without behavior change
- Test name describes HOW not WHAT

```csharp
// BAD: Bypasses HTTP interface to verify via DbContext
[Fact]
public async Task CreateResume_ShouldSaveToDatabase()
{
    await app.Client
        .WithAuthToken(token)
        .POSTAsync<CreateResumeEndpoint, CreateResumeRequest>(request);

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ResumesDbContext>();
    var resume = await db.Resumes.FirstOrDefaultAsync(r => r.UserId == userId);
    resume.ShouldNotBeNull();
}

// GOOD: Verifies through HTTP interface
[Fact]
public async Task CreateResume_ShouldBeRetrievableAfterCreation()
{
    var (_, createResponse) = await app.Client
        .WithAuthToken(token)
        .POSTAsync<CreateResumeEndpoint, CreateResumeRequest, CreateResumeResponse>(request);

    var (httpResponse, getResponse) = await app.Client
        .WithAuthToken(token)
        .GETAsync<GetResumeEndpoint, GetResumeRequest, GetResumeResponse>(
            new GetResumeRequest(createResponse.Id));

    httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    getResponse.Id.ShouldBe(createResponse.Id);
}
```

## Test Data Patterns

**ClassData with seeded fakers** for generated data:

```csharp
// Data/UnregisteredUsersData.cs
public class UnregisteredUsersData : IEnumerable<TheoryDataRow<SignInRequest>>
{
    public IEnumerator<TheoryDataRow<SignInRequest>> GetEnumerator()
    {
        var faker = new SignInRequestFaker(seed: 42);
        foreach (var request in faker.Generate(3))
            yield return new TheoryDataRow<SignInRequest>(request);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

**InlineData** only for simple literals:

```csharp
[Theory]
[InlineData("", "")]
[InlineData("  ", "  ")]
public async Task EmptyFields_ShouldReturnBadRequest(string username, string password)
```