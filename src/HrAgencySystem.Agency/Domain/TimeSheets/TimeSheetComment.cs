using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Agency.Domain.TimeSheets;

/// <summary>
/// A line on the sheet's thread. The author's standing at the time is frozen into
/// <paramref name="AuthorRole"/> - "returned by the supervisor" has to keep meaning that after a
/// reorganisation moves them somewhere else.
/// </summary>
public sealed record TimeSheetComment(
    UserSnapshot Author,
    TimeSheetRole AuthorRole,
    string Content,
    DateTimeOffset At
);

/// <summary>Which hat somebody was wearing, not which role they hold today.</summary>
public enum TimeSheetRole
{
    Owner,
    Supervisor,
    Payroll,
}
