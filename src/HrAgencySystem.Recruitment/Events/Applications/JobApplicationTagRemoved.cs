using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationTagRemoved(Guid JobApplicationId, Tag Tag, UserSnapshot RemovedBy, DateTimeOffset ModifiedAt);