using System.Text.Json;
using Faber.Modules.Auth.Application.Features.ForgotPassword;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Modules.Auth.Application.Features.SignUp;
using Faber.Modules.Auth.Application.Tests.Features.Shared;
using Faber.Modules.Users.PublicApi.Contracts;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(
    typeof(RecordSerializer),
    typeof(CreateUserRequest),
    typeof(ForgotPasswordRequest),
    typeof(SignInRequest),
    typeof(SignUpRequest))]

namespace Faber.Modules.Auth.Application.Tests.Features.Shared;

public class RecordSerializer : IXunitSerializer
{
    private static readonly HashSet<Type> SupportedTypes =
    [
        typeof(CreateUserRequest),
        typeof(ForgotPasswordRequest),
        typeof(SignInRequest),
        typeof(SignUpRequest)
    ];

    public bool IsSerializable(Type type, object? value, out string failureReason)
    {
        if (SupportedTypes.Contains(type))
        {
            failureReason = string.Empty;

            return true;
        }

        failureReason = $"Type '{type.FullName}' is not supported by {nameof(RecordSerializer)}";

        return false;
    }

    public string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, value.GetType());
    }

    public object Deserialize(Type type, string serializedValue)
    {
        return JsonSerializer.Deserialize(serializedValue, type) ??
               throw new InvalidOperationException($"Failed to deserialize '{type.FullName}'");
    }
}