using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Recruitment.Application.JobApplications.Notes.Delete;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapDeleteNote
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/recruitment/job-applications/{applicationId}/{noteId}/note
        group.MapDelete("{applicationId:guid}/note/{noteId:guid}", Handler).WithSummary("Delete note")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IMessageBus bus,
        Guid applicationId,
        Guid noteId,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<JobApplicationNoteDeleted>(
                new DeleteNote(applicationId, user.OrganizationId, noteId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}