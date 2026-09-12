using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobApplications.ChangeStatus;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;


internal static class MapChangeStatus
{
    // PUT /api/recruitment/job-applications/{id}/status
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{jobApplicationId}/status", Handler)
            .WithSummary("Change status")
            .WithName("Change job application status")
            .Produces<JobApplicationStatusChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user, Guid jobApplicationId,
        ChangeJobApplicationStatusRequest request)
    {
        var result =
            await bus.InvokeAsync<ChangeJobApplicationStatusResult>(request.ToCommand(jobApplicationId,
                user.OrganizationId, user.UserId));

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangeJobApplicationStatusRequest(
    JobApplicationUpdateStatus Status,
    string? Note,
    Guid? InterviewId
)
{
    public ChangeJobApplicationStatus ToCommand(Guid jobApplicationId, Guid organizationId, Guid userId)
        => new (jobApplicationId, organizationId, Note ?? "", Status, InterviewId, userId);
}