namespace Faber.Bootstrap;

internal static class ResponseValidation
{
    public static void EnsureSuccess(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new HttpRequestException(
            $"Failed to {operation}: HTTP {(int)response.StatusCode} ({response.StatusCode}).",
            null,
            response.StatusCode);
    }
}
