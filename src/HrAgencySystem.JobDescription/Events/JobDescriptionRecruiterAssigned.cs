using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionRecruiterAssigned(UserSnapshot Recruiter, UserSnapshot ModifiedBy,  DateTimeOffset OccurredAt);