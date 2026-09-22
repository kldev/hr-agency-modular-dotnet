namespace HrAgencySystem.Agency.Domain.TimeSheets;

/// <summary>
/// Where a sheet is in its month.
/// <para>
/// There is no "rejected". Hours are not refused - they are either sent back to be corrected or
/// they are accepted. That is the one place this deliberately parts ways with leave requests, where
/// "no, not that week" is a real answer; a month somebody has already worked has no such answer.
/// </para>
/// </summary>
public enum TimeSheetStatus
{
    /// <summary>Being filled in. Editable.</summary>
    Draft,

    /// <summary>Sent to the supervisor. Frozen for the person it belongs to.</summary>
    Submitted,

    /// <summary>The supervisor accepted the hours. Waiting for payroll.</summary>
    Approved,

    /// <summary>Sent back with a reason. Editable again.</summary>
    Correction,

    /// <summary>Handed over to payroll. The end of the line.</summary>
    Settled,
}
