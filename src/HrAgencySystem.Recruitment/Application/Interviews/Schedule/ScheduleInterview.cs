using System.Text.Json.Serialization;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Extensions;

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
    [JsonIgnore]
    public DateTimeOffset ScheduledAtInstant
        => ScheduledAt.ToInstantUtc(ScheduledTimezone);


}