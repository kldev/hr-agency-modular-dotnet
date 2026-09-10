using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Recruitment.Application.Interviews.Schedule;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;

internal static class MapSchedule
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/interviews/schedule
        group.MapPost("schedule", Handler)
            .WithSummary("Schedule interview")
            .Produces<InterviewCreated>()
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IMessageBus bus, ScheduleInterviewRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<InterviewCreated>(
            request.ToCommand(user.OrganizationId, user.UserId), ct);
        return TypedResults.Created($"/api/interviews/{result.InterviewId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ScheduleInterviewRequest(
    Guid JobApplicationId,
    DateTime ScheduledAt,
    InterviewFormat Format,
    InterviewType InterviewType,
    string Note,
    Guid InterviewerId,
    string ScheduledTimezone = "Europe/Warsaw")
{
    public ScheduleInterview ToCommand(Guid organizationId, Guid createdBy)
    {
        return new ScheduleInterview(JobApplicationId,
            organizationId,
            ScheduledAt, Format,
            InterviewType, Note, InterviewerId, createdBy, ScheduledTimezone);
    }
}