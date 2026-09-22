using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Application.TimeSheets.Export;

/// <summary>
/// One person's month as payroll reads it. <paramref name="Amount"/> is null whenever it cannot be
/// worked out from hours alone - no rate, or a rate that is not hourly - and that row is exactly
/// the one somebody has to look at.
/// <para>
/// <paramref name="ContractType"/> is null for somebody whose employment record is gone or never
/// existed; their hours are still agreed and still belong in the file.
/// </para>
/// </summary>
public sealed record SettlementRow(
    UserSnapshot User,
    WorkerContractType? ContractType,
    TimeSheetStatus Status,
    int Minutes,
    decimal DecimalHours,
    WorkRate? Rate,
    decimal? Amount,
    IReadOnlyList<WorkDay> Days
);

/// <summary>
/// The bottom line. The amount is only a sum when every amount is in one currency - adding zloty
/// to euro gives a number, not an answer - so a mixed file leaves it empty.
/// </summary>
public sealed record SettlementTotal(
    int Minutes,
    decimal DecimalHours,
    decimal? Amount,
    string? Currency
);
