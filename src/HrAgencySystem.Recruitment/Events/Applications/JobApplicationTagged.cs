using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Events.Applications;

public sealed record JobApplicationTagged(Guid JobApplicationId, Tag Tag, UserSnapshot Author, DateTimeOffset CreatedAt);