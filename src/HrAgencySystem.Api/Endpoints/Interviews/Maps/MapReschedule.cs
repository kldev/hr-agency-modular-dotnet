using System.ComponentModel;
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
            .MapPut(ApiEndpoints.Interviews.Reschedule, Handler)
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
        [property: Description(
            "The local date and time of the interview, without an offset - read in ScheduledTimezone and stored as an instant, so 10:00 in Europe/Warsaw stays 10:00 there across daylight saving."
        )]
            DateTime ScheduledAt,
        [property: Description(
            "Why it moved, or anything the interviewer should know. May be empty."
        )]
            string Note,
        [property: Description(
            "IANA time zone ScheduledAt is in, e.g. \"Europe/Warsaw\" (the default)."
        )]
            string ScheduledTimezone = "Europe/Warsaw",
        [property: Description("Where to go, for an OnSite interview.")] string Location = "",
        [property: Description("The meeting link, for an Online interview.")] string MeetingUrl = ""
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
