using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Web.Services;

/// <summary>
/// Whether the API answers - the one dependency this host has. Asks the API's own <c>/healthz</c>,
/// without the key, so a revoked key and a dead API show up as different things. Read by both the
/// board's <c>/healthz</c> and <c>/health/ready</c>.
/// </summary>
public sealed class ApiHealthProbe(IHttpClientFactory clients, IOptions<InternalApiConfig> config)
    : IHealthCheck
{
    public const string Up = "UP";

    public async Task<string> CheckAsync(CancellationToken ct)
    {
        var client = clients.CreateClient(JobBoardClientRegistration.ApiHealthClient);
        client.Timeout = TimeSpan.FromSeconds(3);

        try
        {
            var response = await client.GetAsync(
                new Uri(new Uri(config.Value.BaseUrl.TrimEnd('/') + "/"), "healthz"),
                ct
            );

            return response.IsSuccessStatusCode ? Up : $"DOWN ({(int)response.StatusCode})";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return "DOWN";
        }
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var api = await CheckAsync(cancellationToken);
        return api == Up ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy($"API {api}");
    }
}
