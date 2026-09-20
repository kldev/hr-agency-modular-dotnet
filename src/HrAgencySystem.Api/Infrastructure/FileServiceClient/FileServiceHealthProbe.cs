using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

/// <summary>
/// Answers whether the file service is reachable. It exists because documents are the one thing this
/// API cannot serve on its own, and "the second process is not running" should be visible on the
/// health endpoint rather than discovered by the first person who tries to upload something.
/// </summary>
public sealed class FileServiceHealthProbe(
    IHttpClientFactory factory,
    IOptions<FileServiceClientConfig> options
)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    public async Task<string> CheckAsync(CancellationToken ct)
    {
        var baseUrl = options.Value.BaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
            return "NOT_CONFIGURED";

        using var client = factory.CreateClient();
        client.Timeout = Timeout;

        try
        {
            var response = await client.GetAsync(new Uri(new Uri(baseUrl), "/healthz"), ct);
            return response.IsSuccessStatusCode ? "UP" : "DOWN";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return "DOWN";
        }
    }
}
