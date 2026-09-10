using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Owner.Maps;

internal static class MapGet
{
    public static void Map(RouteGroupBuilder group)
    {
        group.Map("/api/owners/{ownerId:guid}", Handler)
            .WithSummary("Get owner")
            .Produces<OwnerProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IDocumentSession session, Guid ownerId, CancellationToken ct)
    {
        var result = await session.Query<OwnerProjection>()
            .Where(z => z.Id == ownerId).SingleOrDefaultAsync(ct);

        if (result == null)
            return TypedResults.NotFound(new ProblemDetails()
            {
                Title = "Owner not found",
                Status = StatusCodes.Status404NotFound, Detail = $"Owner with {ownerId} id not found"
            });
        
        return TypedResults.Ok(result);
    }
}