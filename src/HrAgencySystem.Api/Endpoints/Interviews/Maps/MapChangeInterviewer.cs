using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Interviews.ChangeInterviewer;
using HrAgencySystem.Recruitment.Events.Interviews;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapChangeInterviewer
{
    // PUT /api/interviews/{id}/interviewer
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Interviews.ChangeInterviewer, Handler)
            .WithSummary("Change interviewer")
            .WithName("Change interviewer")
            .Produces<InterviewerChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid interviewId,
        ChangeInterviewerRequest request
    )
    {
        var result = await bus.InvokeAsync<InterviewerChanged>(
            request.ToCommand(interviewId, user.OrganizationId, user.UserId)
        );

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangeInterviewerRequest(
    [property: Description("The user who runs the interview from now on.")] Guid InterviewerId,
    [property: Description("Optional note on the change.")] string? Note
)
{
    public ChangeInterviewer ToCommand(Guid interviewId, Guid organizationId, Guid userId) =>
        new(interviewId, organizationId, InterviewerId, Note ?? "", userId);
}
