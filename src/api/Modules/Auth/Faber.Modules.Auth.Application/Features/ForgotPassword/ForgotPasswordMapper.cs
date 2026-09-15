namespace Faber.Modules.Auth.Application.Features.ForgotPassword;

public static class ForgotPasswordMapper
{
    public static ForgotPasswordCommand MapToCommand(this ForgotPasswordRequest request)
    {
        return new ForgotPasswordCommand(request.Email);
    }
}