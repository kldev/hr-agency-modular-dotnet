using HrAgencySystem.ReportsService.Contracts;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Infrastructure.ReportsClient;

/// <summary>Whether the reports service answers - the file service probe's twin.</summary>
public sealed class ReportsHealthProbe(
    IHttpClientFactory factory,
    IOptions<ReportsClientConfig> options
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
            var response = await client.GetAsync(
                new Uri(new Uri(baseUrl), ReportsServiceRoutes.Health),
                ct
            );
            return response.IsSuccessStatusCode ? "UP" : "DOWN";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return "DOWN";
        }
    }
}
