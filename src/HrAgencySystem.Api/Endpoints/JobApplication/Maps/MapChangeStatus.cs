using System.ComponentModel;
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
        group
            .MapPut(ApiEndpoints.Recruitment.JobApplications.ChangeStatus, Handler)
            .WithSummary("Change status")
            .WithName("Change job application status")
            .Produces<JobApplicationStatusChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid jobApplicationId,
        ChangeJobApplicationStatusRequest request
    )
    {
        var result = await bus.InvokeAsync<ChangeJobApplicationStatusResult>(
            request.ToCommand(jobApplicationId, user.OrganizationId, user.UserId)
        );

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangeJobApplicationStatusRequest(
    [property: Description(
        "Screening, Interview, Assessment, Offer, Hired, Rejected or Withdrawn. Which moves are allowed depends on the current status."
    )]
        JobApplicationUpdateStatus Status,
    [property: Description("Optional note on the decision.")] string? Note,
    [property: Description(
        "The interview the application moves to - required when Status is Interview, ignored otherwise."
    )]
        Guid? InterviewId
)
{
    public ChangeJobApplicationStatus ToCommand(
        Guid jobApplicationId,
        Guid organizationId,
        Guid userId
    ) => new(jobApplicationId, organizationId, Note ?? "", Status, InterviewId, userId);
}
