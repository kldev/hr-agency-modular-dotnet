using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Interviews.Schedule;

public sealed record ScheduleInterview(
    Guid JobApplicationId,
    OrganizationId OrganizationId,
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