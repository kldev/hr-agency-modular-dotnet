using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.ValueObjects;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Positions.Update;

public sealed record UpdatePosition(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    Guid PositionId,
    string Name,
    string? ContractName,
    string? WorkDescription,
    IReadOnlyList<string>? Duties,
    IReadOnlyList<string>? RequiredQualifications,
    WorkerContractType ContractType,
    decimal? RateAmount,
    string? RateCurrency,
    RateUnit RateUnit,
    RateBasis RateBasis,
    string? Street,
    string? BuildingNumber,
    string? UnitNumber,
    string? PostalCode,
    string? City,
    string? CountryCode,
    decimal? WeeklyHours,
    TimeOnly? WorkStartsAt,
    string? WorkSchedule,
    int? PayoutDay,
    string? ProbationPeriod,
    string? NoticePeriod,
    IReadOnlyList<string>? Allowances,
    int? PlannedHeadcount,
    EngagementType? DefaultEngagementType,
    Guid ModifiedBy
) : IUpdateCommand, IPositionData;
