using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.JobApplication;

public sealed record JobApplicationTagRemoved(Guid JobApplicationId, Tag Tag, UserSnapshot RemovedBy, DateTimeOffset ModifiedAt);