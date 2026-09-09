using HrAgencySystem.Api.Auth;
using HrAgencySystem.JobDescription.Application.ChangeStatus;
using HrAgencySystem.JobDescription.Domain;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapUpdateStatus
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/api/job-description/{jobDescriptionId:guid}/{status}", Handler).WithDescription("").WithSummary("Update status");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, Guid jobDescriptionId, JobDescriptionStatus status,
        IMessageBus bus, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<UpdateJobDescriptionStatusResult>(
            new ChangeJobDescriptionStatus(jobDescriptionId, status,  user.UserId, user.OrganizationId), ct);

        return TypedResults.Ok(result);
    }
}
