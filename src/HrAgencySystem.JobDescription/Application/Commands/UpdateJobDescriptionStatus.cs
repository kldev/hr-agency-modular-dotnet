using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.JobDescription.Application.Commands;

public sealed record UpdateJobDescriptionStatus(Guid JobDescriptionId, JobDescriptionStatus Status,  Guid ModifiedBy, Guid OrganizationId): IUpdateCommand;
