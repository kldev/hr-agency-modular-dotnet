using System.Net;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.JobDescription.Application.Port;
using HrAgencySystem.JobDescription.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/job-description/{jobDescriptionId:guid}", Handler).WithSummary("Get job description");
    }

    private static async Task<IResult> Handler(IJobDescriptionQueryRepository repository, AppUserAuthenticated user, Guid jobDescriptionId, CancellationToken ct)
    {
        var result = await repository.GetJobDescription(user.OrganizationId, jobDescriptionId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Job description", jobDescriptionId));
        }
        
        return TypedResults.Ok(result);
    }
}