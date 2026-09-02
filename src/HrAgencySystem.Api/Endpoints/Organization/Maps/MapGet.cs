using HrAgencySystem.Organization.Events;
using Marten;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/organization", Handler).WithSummary("Get all organizations");
    }

    private static async Task<IResult> Handler(IDocumentSession session, CancellationToken ct)
    {
        var result = await session.Query<OrganizationCreated>().ToListAsync(ct);

        return TypedResults.Ok(result);
    }
}