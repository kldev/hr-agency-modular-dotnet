using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.JobDescription.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapGetStatusHistory
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/job-description/status", Handler)
            .Produces<IReadOnlyList<JdStatusChangeHistory>>()
            .ProducesStandardErrors()
            .WithSummary("Get statuses history")
            .WithName("Get job-description statuses history");
    }

    private static async Task<IResult> Handler(IDocumentSession session, AppUserAuthenticated user,
        Guid? jobDescriptionId,
        CancellationToken ct)
    {
        var query = session.Query<JdStatusChangeHistory>()
            .Where(z => z.OrgId == user.OrganizationId);

        if (jobDescriptionId.HasValue && jobDescriptionId != Guid.Empty)
        {
            query = query.Where(z => z.JobDescriptionId == jobDescriptionId.Value);
        }

        var result = await query.ToListAsync(ct);

        return TypedResults.Ok(result);
    }
}