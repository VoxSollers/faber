# Interface Design for Testability

Good interfaces make testing natural:

1. **Accept dependencies via DI, don't create them**

   ```csharp
   // Testable — primary constructor DI
   public class SignInEndpoint(IIdentityModuleApi identityApi) : Endpoint<SignInRequest, SignInResponse>
   {
       public override async Task HandleAsync(SignInRequest req, CancellationToken ct)
       {
           var result = await identityApi.SignInAsync(req.ToCommand(), ct);
           // ...
       }
   }

   // Hard to test — creates dependency internally
   public class SignInEndpoint : Endpoint<SignInRequest, SignInResponse>
   {
       public override async Task HandleAsync(SignInRequest req, CancellationToken ct)
       {
           var client = new HttpClient();
           var result = await client.PostAsync("http://keycloak/...", ...);
       }
   }
   ```

2. **Return `ErrorOr<T>`, don't throw exceptions**

   ```csharp
   // Testable — result is inspectable
   public async Task<ErrorOr<SignInResponse>> SignInAsync(SignInCommand command, CancellationToken ct)
   {
       if (user is null)
           return Error.NotFound("User not found");
       return new SignInResponse(token);
   }

   // Hard to test — must catch exceptions
   public async Task<SignInResponse> SignInAsync(SignInCommand command, CancellationToken ct)
   {
       if (user is null)
           throw new NotFoundException("User not found");
       return new SignInResponse(token);
   }
   ```

3. **Small surface area on Module APIs**
   - Fewer methods in `I{Module}ModuleApi` = fewer test doubles needed
   - Fewer parameters = simpler test setup
   - Use records for grouping related parameters