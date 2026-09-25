using HrAgencySystem.Web.Services;

namespace HrAgencySystem.Web.Endpoints.Public.Maps;

/// <summary>
/// Whether this host can do its job, which since it stopped talking to the database means whether
/// the API answers. Reports "api: DOWN" only for a dead API, not for a revoked key - see
/// <see cref="ApiHealthProbe"/>.
/// </summary>
internal static class MapHealth
{
    internal static void Map(RouteGroupBuilder group) => group.MapGet("healthz", Handler);

    private static async Task<IResult> Handler(ApiHealthProbe probe, CancellationToken ct) =>
        TypedResults.Ok(new { status = "UP", api = await probe.CheckAsync(ct) });
}
