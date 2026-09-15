using System.Text.Json.Serialization;

namespace Faber.Modules.Identity.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionTokenType
{
    VerifyEmail,
    ForgotPassword
}