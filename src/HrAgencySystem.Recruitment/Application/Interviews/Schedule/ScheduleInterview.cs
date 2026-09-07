using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Interviews.Schedule;

public sealed record ScheduleInterview(
    Guid JobApplicationId,
    Guid OrganizationId,
    DateTime ScheduledAt,
    InterviewFormat Format,
    InterviewType InterviewType,
    string Note,
    Guid InterviewerId,
    Guid CreatedBy,
    string ScheduledTimezone = "Europe/Warsaw") : ICreateCommand
{
    public DateTimeOffset ScheduledAtInstant
    {
        get
        {
            var timezone =
                TimeZoneInfo.FindSystemTimeZoneById(ScheduledTimezone);

            var utc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(
                    ScheduledAt,
                    DateTimeKind.Unspecified),
                timezone);

            return new DateTimeOffset(utc);
        }
    }
}