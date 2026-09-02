using HrAgencySystem.JobDescription.Domain;

namespace HrAgencySystem.JobDescription.Application.Commands;

public sealed record UpdateJobDescriptionStatus(Guid JobDescriptionId, JobDescriptionStatus Status,  Guid ModifiedBy, Guid OrganizationId);
