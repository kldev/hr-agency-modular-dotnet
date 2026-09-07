using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Recruitment.Application.JobApplication.Notes.Create;
using HrAgencySystem.Recruitment.Events.Applications;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapCreateNote
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/recruitment/job-applications/{applicationId}/{noteId}/note
        group.MapPost("{applicationId:guid}/note", Handler).WithSummary("Add note")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetailsOptions>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IMessageBus bus, Guid applicationId,
        CreateNoteRequest request, CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<JobApplicationNoteAdded>(
                new CreateNote(applicationId, user.OrganizationId, request.Note, user.UserId), ct);

        return TypedResults.Created($"/api/recruitment/job-applications/{applicationId}/notes", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CreateNoteRequest(string Note);