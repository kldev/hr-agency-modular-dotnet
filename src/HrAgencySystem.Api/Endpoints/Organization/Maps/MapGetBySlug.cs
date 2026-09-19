using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.Organization.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapGetBySlug
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Organizations.GetBySlug, Handler)
            .WithSummary("Get organization by Slug")
            .WithName("Get organization by Slug")
            .Produces<OrganizationProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IDocumentSession session,
        string slug,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<OrganizationProjection>()
            .Where(z => z.Slug == slug)
            .OrderByDescending(z => z.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (result == null)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Organization", slug));

        return TypedResults.Ok(result);
    }
}
