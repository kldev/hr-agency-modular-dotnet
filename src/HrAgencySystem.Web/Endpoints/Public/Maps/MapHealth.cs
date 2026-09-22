using HrAgencySystem.Web.Services;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Web.Endpoints.Public.Maps;

/// <summary>
/// Whether this host can do its job, which since it stopped talking to the database means whether
/// the API answers. Asks the API's own <c>/healthz</c>, without the key, so a revoked key and a
/// dead API show up as different things: this reports "api: DOWN" only for the second.
/// </summary>
internal static class MapHealth
{
    internal static void Map(RouteGroupBuilder group) =>
        group.MapGet("healthz", Handler);

    private static async Task<IResult> Handler(
        IHttpClientFactory clients,
        IOptions<InternalApiConfig> config,
        CancellationToken ct
    )
    {
        var client = clients.CreateClient(JobBoardClientRegistration.ApiHealthClient);
        client.Timeout = TimeSpan.FromSeconds(3);

        string api;

        try
        {
            var response = await client.GetAsync(
                new Uri(new Uri(config.Value.BaseUrl.TrimEnd('/') + "/"), "healthz"),
                ct
            );

            api = response.IsSuccessStatusCode ? "UP" : $"DOWN ({(int)response.StatusCode})";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            api = "DOWN";
        }

        return TypedResults.Ok(new { status = "UP", api });
    }
}
