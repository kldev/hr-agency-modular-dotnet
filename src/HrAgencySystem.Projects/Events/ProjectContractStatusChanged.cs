using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectContractStatusChanged(
    Guid ProjectId,
    Guid OrganizationId,
    ContractStatus PreviousStatus,
    ContractStatus Status,
    DateOnly? SignedOn,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
