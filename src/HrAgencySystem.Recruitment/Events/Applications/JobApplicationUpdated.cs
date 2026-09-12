using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationUpdated(
    Guid JobApplicationId,
    DateTimeOffset OccurredAt,
    string FirstName,
    string LastName,
    string Phone,
    UserSnapshot Author):IJobApplicationEvent
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}