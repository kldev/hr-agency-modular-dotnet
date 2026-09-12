using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Interviews.ChangeStatus;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;


internal static class MapChangeStatus
{
    // PUT /api/interviews/{id}/status
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{interviewId}/status", Handler)
            .WithSummary("Change status")
            .WithName("Change interview status")
            .Produces<InterviewStatusChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user, Guid interviewId,
        ChangeInterviewStatusRequest request)
    {
        var result =
            await bus.InvokeAsync<InterviewStatusChanged>(request.ToCommand(interviewId,
                user.OrganizationId, user.UserId));

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangeInterviewStatusRequest(
    InterviewStatus Status,
    string? Note
)
{
    public ChangeInterviewStatus ToCommand(Guid interviewId, Guid organizationId, Guid userId)
        => new (interviewId, organizationId, Note ?? "", Status,  userId);
}