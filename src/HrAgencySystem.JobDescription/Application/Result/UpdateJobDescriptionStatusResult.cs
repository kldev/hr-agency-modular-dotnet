using HrAgencySystem.JobDescription.Domain;

namespace HrAgencySystem.JobDescription.Application.Result;

public sealed record UpdateJobDescriptionStatusResult(Guid JobDescriptionId, JobDescriptionStatus Status);