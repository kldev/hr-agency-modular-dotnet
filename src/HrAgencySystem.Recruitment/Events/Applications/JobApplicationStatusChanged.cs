using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationStatusChanged(
    Guid JobApplicationId,
    Guid CandidateId,
    DateTimeOffset OccurredAt,
    JobApplicationStatus OldStatus,
    JobApplicationStatus NewStatus,
    UserSnapshot Author) : IJobApplicationEvent;