using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Add;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapTagList
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/recruitment/job-applications/{id}/tag-list
        group
            .MapPut(ApiEndpoints.Recruitment.JobApplications.TagList, Handler)
            .WithSummary("Add multiple tag")
            .WithName("Add multiple tag to application")
            .Produces<JobApplicationTagged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid applicationId,
        TagRequestList request,
        CancellationToken ct
    )
    {
        JobApplicationTagged? result = null;
        foreach (var tag in request.TagIds)
        {
            result = await bus.InvokeAsync<JobApplicationTagged>(
                new TagApplication(tag, applicationId, user.OrganizationId, user.UserId),
                ct
            );
        }

        return TypedResults.Ok(result);
    }
}
