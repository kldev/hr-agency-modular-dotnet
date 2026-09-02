using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionRecruiterAssigned(UserSnapshot Recruiter,  DateTimeOffset OccurredAt);