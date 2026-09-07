using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.JobApplication.ChangeJobApplicationStatus;
using HrAgencySystem.Recruitment.Domain.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;


internal static class MapChangeStatus
{
    // PUT /api/recruitment/job-applications/{id}/status
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{jobApplicationId}/status", Handler).WithSummary("Change job application status");
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