using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Application.Employment.Start;

public sealed record StartAgencyEmployment(
    Guid OrganizationId,
    Guid UserId,
    WorkerContractType ContractType,
    DateOnly StartsOn,
    decimal? WeeklyHours,
    Guid StartedBy
);
