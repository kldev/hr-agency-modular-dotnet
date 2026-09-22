using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Events;

/// <summary>Somebody starts working for us, on a contract of a named kind.</summary>
public sealed record AgencyEmploymentStarted(
    Guid OrganizationId,
    Guid UserId,
    UserSnapshot User,
    WorkerContractType ContractType,
    DateOnly StartsOn,
    decimal? WeeklyHours,
    UserSnapshot StartedBy,
    DateTimeOffset StartedAt
);

/// <summary>
/// The terms change without the engagement ending - a mandate turning into an employment contract,
/// or a change of hours. The old terms are kept, because a year spent on two contracts is still one
/// year of somebody's history.
/// </summary>
public sealed record AgencyEmploymentTermsChanged(
    Guid OrganizationId,
    Guid UserId,
    WorkerContractType ContractType,
    DateOnly EffectiveFrom,
    decimal? WeeklyHours,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

/// <summary>
/// The engagement ends on a given day. The record stays - it still stands behind every month this
/// person filled in while it ran.
/// </summary>
public sealed record AgencyEmploymentEnded(
    Guid OrganizationId,
    Guid UserId,
    DateOnly EndsOn,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
