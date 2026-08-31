using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using Marten;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapGetOrganizationBySlug
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/organization/{slug}", Handler).WithSummary("Get organization by Slug");
    }
    
    private static async Task<IResult> Handler(IDocumentSession session, string slug, CancellationToken ct)
    {
        var result = await session.Query<OrganizationCreated>()
            .Where(z => z.Slug == slug).OrderByDescending(z=>z.CreatedAt).ToListAsync(ct);

        return TypedResults.Ok(result.FirstOrDefault());
    }
}