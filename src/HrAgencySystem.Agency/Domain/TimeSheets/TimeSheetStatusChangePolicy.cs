namespace HrAgencySystem.Agency.Domain.TimeSheets;

/// <summary>
/// The whole flow in one table, the same shape as <c>ProjectStatusChangePolicy</c>. Conditions that
/// need something outside the graph - who is asking, whether the month has any hours on it - stay
/// in the handlers.
/// </summary>
public static class TimeSheetStatusChangePolicy
{
    private static readonly Dictionary<TimeSheetStatus, TimeSheetStatus[]> Allowed = new()
    {
        [TimeSheetStatus.Draft] = [TimeSheetStatus.Submitted],
        [TimeSheetStatus.Submitted] = [TimeSheetStatus.Approved, TimeSheetStatus.Correction],

        // Payroll can still send an approved month back; that is the whole reason Correction is
        // reachable from two places.
        [TimeSheetStatus.Approved] = [TimeSheetStatus.Settled, TimeSheetStatus.Correction],
        [TimeSheetStatus.Correction] = [TimeSheetStatus.Submitted],
        [TimeSheetStatus.Settled] = [],
    };

    /// <summary>The two states the person who owns the sheet may type into.</summary>
    public static bool IsEditable(TimeSheetStatus status) =>
        status is TimeSheetStatus.Draft or TimeSheetStatus.Correction;

    public static bool CanChange(TimeSheetStatus from, TimeSheetStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
