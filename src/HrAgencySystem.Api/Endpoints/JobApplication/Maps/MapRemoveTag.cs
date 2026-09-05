using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Recruitment.Application.JobApplication.RemoveApplicationTag;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapRemoveTag
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/recruitment/job-applications/{id}/tag
        group.MapDelete("{applicationId:guid}/tag/{tagId:guid}", Handler).WithSummary("Remove tag from job application")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IMessageBus bus, 
        Guid applicationId,
        Guid tagId,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobApplicationTagRemoved>(new RemoveApplicationTag(applicationId, tagId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}