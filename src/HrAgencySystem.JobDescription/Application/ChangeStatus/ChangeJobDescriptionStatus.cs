using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.JobDescription.Application.ChangeStatus;

public sealed record ChangeJobDescriptionStatus(Guid JobDescriptionId, JobDescriptionStatus Status,  Guid ModifiedBy, Guid OrganizationId): IUpdateCommand;
