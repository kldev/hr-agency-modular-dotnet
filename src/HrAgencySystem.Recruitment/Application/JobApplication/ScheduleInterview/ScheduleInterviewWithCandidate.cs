using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.ScheduleInterview;

public sealed record ScheduleInterviewWithCandidate(
    Guid JobApplicationId,
    DateTime ScheduledAt,
    Guid ModifiedBy,
    string ScheduledTimezone = "Europe/Warsaw") : IUpdateCommand
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