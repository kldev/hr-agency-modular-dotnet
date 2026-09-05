using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.JobApplication.TagApplication;
using HrAgencySystem.Recruitment.Events.Applications;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapTag
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/recruitment/job-applications/{id}/tag
        group.MapPut("{applicationId:guid}/tag", Handler).WithSummary("Tag job application")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IMessageBus bus, 
        Guid applicationId,TagRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobApplicationTagged>(new TagApplication(request.TagId, applicationId, user.OrganizationId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}

