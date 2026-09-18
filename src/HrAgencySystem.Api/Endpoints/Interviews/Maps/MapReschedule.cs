using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Interviews.Reschedule;
using HrAgencySystem.Recruitment.Events.Interviews;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapReschedule
{
    // PUT /api/interviews/{id}/reschedule
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut("{interviewId}/reschedule", Handler)
            .WithSummary("Reschedule interview")
            .WithName("Reschedule interview")
            .Produces<InterviewRescheduled>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid interviewId,
        RescheduleInterviewRequest request
    )
    {
        var result = await bus.InvokeAsync<InterviewRescheduled>(
            request.ToCommand(interviewId, user.OrganizationId, user.UserId)
        );

        return TypedResults.Ok(result);
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed record RescheduleInterviewRequest(
        DateTime ScheduledAt,
        string Note,
        string ScheduledTimezone = "Europe/Warsaw",
        string Location = "",
        string MeetingUrl = ""
    )
    {
        public RescheduleInterview ToCommand(
            Guid interviewId,
            Guid organizationId,
            Guid modifiedBy
        ) =>
            new(
                interviewId,
                organizationId,
                Note,
                ScheduledAt,
                modifiedBy,
                ScheduledTimezone,
                Location,
                MeetingUrl
            );
    }
}
