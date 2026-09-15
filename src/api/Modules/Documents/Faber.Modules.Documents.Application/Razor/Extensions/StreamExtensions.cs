namespace Faber.Modules.Documents.Application.Razor.Extensions;

public static class StreamExtensions
{
    public static async Task<byte[]> ToArrayAsync(this Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }

    public static async Task<string> ToBase64StringAsync(this Stream stream)
    {
        return Convert.ToBase64String(await stream.ToArrayAsync());
    }
}