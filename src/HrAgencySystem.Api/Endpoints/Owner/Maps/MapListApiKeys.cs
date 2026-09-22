using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Port;

namespace HrAgencySystem.Api.Endpoints.Owner.Maps;

internal static class MapListApiKeys
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Owners.ApiKeys, Handler)
            .WithSummary("Every key ever issued, revoked ones included - never their values")
            .WithName("List service API keys")
            .Produces<IReadOnlyList<ServiceApiKeyRow>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IServiceApiKeyRepository keys, CancellationToken ct)
    {
        var all = await keys.ListAsync(ct);

        return TypedResults.Ok(
            all.Select(key => new ServiceApiKeyRow(
                    key.Id,
                    key.Name,
                    key.DisplayPrefix,
                    key.CreatedAt,
                    key.CreatedBy,
                    key.RevokedAt,
                    key.RevokedBy
                ))
                .ToList()
        );
    }

    /// <summary>What a list may show: the hash stays in the database, the value was never kept.</summary>
    internal sealed record ServiceApiKeyRow(
        Guid Id,
        string Name,
        string DisplayPrefix,
        DateTimeOffset CreatedAt,
        Guid CreatedBy,
        DateTimeOffset? RevokedAt,
        Guid? RevokedBy
    );
}
