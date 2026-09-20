using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Contract.ChangeStatus;

public sealed record ChangeContractStatus(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    ContractStatus Status,
    DateOnly? SignedOn,
    Guid ModifiedBy
) : IUpdateCommand;
