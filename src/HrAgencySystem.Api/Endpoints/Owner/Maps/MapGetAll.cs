using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.Owner.Maps;

internal static class MapGetAll
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/owners", Handler)
            .WithSummary("Get owners")
            .Produces<IReadOnlyList<OwnerProjection>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IDocumentSession session, CancellationToken ct)
    {
        var result = await session.Query<OwnerProjection>().ToListAsync(ct);
        return TypedResults.Ok(result);
    }
}