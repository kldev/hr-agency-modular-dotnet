using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Organization.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;


internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{organizationId:guid}", Handler)
            .WithSummary("Get organization by id")
            .WithName("Get organization by id")
            .Produces<OrganizationProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IDocumentSession session, Guid organizationId, CancellationToken ct)
    {
        var result = await session.Query<OrganizationProjection>()
            .Where(z => z.Id == organizationId)
            .OrderByDescending(z=>z.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (result == null) return TypedResults.NotFound(DomainObjectNotFound.NotFound("Organization",organizationId ));
        
        return TypedResults.Ok(result);
    }
}