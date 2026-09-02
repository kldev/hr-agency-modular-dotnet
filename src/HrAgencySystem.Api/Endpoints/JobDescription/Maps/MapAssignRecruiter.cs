using HrAgencySystem.Api.Auth;
using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapAssignRecruiter
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/api/job-description/{jobDescriptionId:guid}/assign-recruiter", Handler).WithSummary("Assign recruiter");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, Guid jobDescriptionId, AssignRecruiter request,
        IMessageBus bus, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobDescriptionRecruiterAssigned>(
            new AssignRecruiterJobDescription(jobDescriptionId, request.RecruiterId,  user.UserId, user.OrganizationId), ct);

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record AssignRecruiter(Guid RecruiterId);