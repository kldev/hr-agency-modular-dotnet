using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Contract.Record;

public sealed record RecordProjectContract(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    string ContractNumber,
    ContractStatus Status,
    DateOnly? SignedOn,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    Guid ModifiedBy
) : IUpdateCommand;
