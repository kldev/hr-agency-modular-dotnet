using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.JobDescription.Application.AssignRecruiter;
using HrAgencySystem.JobDescription.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapAssignRecruiter
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/api/job-description/{jobDescriptionId:guid}/assign-recruiter", Handler)
            .Produces<JobDescriptionRecruiterAssigned>()
            .ProducesStandardErrors()
            .WithSummary("Assign recruiter");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, Guid jobDescriptionId, AssignRecruiterRequest request,
        IMessageBus bus, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobDescriptionRecruiterAssigned>(
            new AssignJobDescriptionRecruiter(jobDescriptionId, request.RecruiterId,  user.UserId, user.OrganizationId), ct);

        return TypedResults.Ok(result);
    }
}

