using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Remove;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapRemoveTagList
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/recruitment/job-applications/{id}/tag/remove
        group
            .MapPut("{applicationId:guid}/tag/remove", Handler)
            .WithSummary("Remove tag list")
            .WithName("Remove applications tag list")
            .ProducesStandardErrors()
            .Produces<JobApplicationTagRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid applicationId,
        TagRequestList request,
        CancellationToken ct
    )
    {
        JobApplicationTagRemoved? result = null;
        foreach (var tag in request.TagIds)
        {
            result = await bus.InvokeAsync<JobApplicationTagRemoved>(
                new RemoveApplicationTag(applicationId, tag, user.OrganizationId, user.UserId),
                ct
            );
        }

        return TypedResults.Ok(result);
    }
}
