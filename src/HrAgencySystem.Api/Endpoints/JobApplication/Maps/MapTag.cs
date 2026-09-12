using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Add;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapTag
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/recruitment/job-applications/{id}/tag
        group.MapPut("{applicationId:guid}/tag", Handler)
            .WithSummary("Tag application")
            .WithName("Tag job application")
            .Produces<JobApplicationTagged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IMessageBus bus, 
        Guid applicationId,TagRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobApplicationTagged>(new TagApplication(request.TagId, applicationId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}

