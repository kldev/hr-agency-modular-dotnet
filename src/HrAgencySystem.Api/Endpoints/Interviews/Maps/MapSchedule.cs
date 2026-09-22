using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Interviews.Schedule;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapSchedule
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/interviews/schedule
        group
            .MapPost(ApiEndpoints.Interviews.Schedule, Handler)
            .WithSummary("Schedule interview")
            .WithName("Schedule interview")
            .Produces<InterviewCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        ScheduleInterviewRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<InterviewCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );
        return TypedResults.Created($"/api/interviews/{result.InterviewId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ScheduleInterviewRequest(
    [property: Description("The application the interview is for.")] Guid JobApplicationId,
    [property: Description(
        "The local date and time of the interview, without an offset - read in ScheduledTimezone and stored as an instant, so 10:00 in Europe/Warsaw stays 10:00 there across daylight saving."
    )]
        DateTime ScheduledAt,
    [property: Description("Online, OnSite or Phone.")] InterviewFormat Format,
    [property: Description("Hr, Technical, Client or Final.")] InterviewType InterviewType,
    [property: Description("Notes for the interviewer. May be empty.")] string Note,
    [property: Description("The user who runs the interview.")] Guid InterviewerId,
    [property: Description(
        "IANA time zone ScheduledAt is in, e.g. \"Europe/Warsaw\" (the default)."
    )]
        string ScheduledTimezone = "Europe/Warsaw",
    [property: Description("Where to go, for an OnSite interview.")] string Location = "",
    [property: Description("The meeting link, for an Online interview.")] string MeetingUrl = ""
)
{
    public ScheduleInterview ToCommand(Guid organizationId, Guid createdBy)
    {
        return new ScheduleInterview(
            JobApplicationId,
            organizationId,
            ScheduledAt,
            Format,
            InterviewType,
            Note,
            InterviewerId,
            createdBy,
            ScheduledTimezone,
            Location,
            MeetingUrl
        );
    }
}
