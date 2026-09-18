using System.Text.Json.Serialization;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Extensions;

namespace HrAgencySystem.Recruitment.Application.Interviews.Reschedule;

public sealed record RescheduleInterview(
    Guid InterviewId,
    Guid OrganizationId,
    string Note,
    DateTime ScheduledAt,
    Guid ModifiedBy,
    string ScheduledTimezone = "Europe/Warsaw",
    string Location = "",
    string MeetingUrl = ""
) : IUpdateCommand
{
    [JsonIgnore]
    public DateTimeOffset ScheduledAtInstant => ScheduledAt.ToInstantUtc(ScheduledTimezone);
}
