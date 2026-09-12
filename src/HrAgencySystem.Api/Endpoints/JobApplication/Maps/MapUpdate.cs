using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobApplications.Update;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapUpdate
{
    // PUT /api/recruitment/job-applications/{id}/status
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{jobApplicationId:guid}", Handler)
            .WithSummary("Update job applicant")
            .WithName("Update job applicant")
            .Produces<JobApplicationUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IMessageBus bus, 
        AppUserAuthenticated user, 
        Guid jobApplicationId,
        UpdateApplicantRequest request)
    {
        var result =
            await bus.InvokeAsync<JobApplicationUpdated>(request.ToCommand(jobApplicationId,
                user.OrganizationId, user.UserId));

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record UpdateApplicantRequest(
    string Phone,
    string FirstName = "",
    string LastName = "")
{
    public UpdateJobApplication ToCommand(Guid jobApplicationId, Guid organizationId, Guid userId)
        => new (jobApplicationId, OrganizationId.From( organizationId), Phone, FirstName, LastName, userId);
}