using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Agency.Events;

/*
 * Every event carries the person and the month although the stream id is derived from them. That is
 * for the read side: a projection built from these has to be able to say whose month it is without
 * reversing a hash, and the derivation is one-way on purpose.
 */

public sealed record TimeSheetStarted(
    Guid OrganizationId,
    Guid UserId,
    UserSnapshot User,
    int Year,
    int Month,
    UserSnapshot StartedBy,
    DateTimeOffset StartedAt
);

/// <summary>A day is written down. Saving the same date again replaces it rather than adding to it.</summary>
public sealed record WorkDaySaved(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    WorkDay Day,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

public sealed record WorkDayRemoved(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    DateOnly Date,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

public sealed record TimeSheetSubmitted(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    int TotalMinutes,
    UserSnapshot SubmittedBy,
    DateTimeOffset SubmittedAt
);

public sealed record TimeSheetApproved(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    UserSnapshot ApprovedBy,
    DateTimeOffset ApprovedAt
);

/// <summary>
/// Sent back to be filled in again. The reason is part of the event rather than a separate comment
/// command, which is what makes "say what is wrong" impossible to skip.
/// </summary>
public sealed record TimeSheetReturnedForCorrection(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    TimeSheetComment Reason,
    UserSnapshot ReturnedBy,
    DateTimeOffset ReturnedAt
);

/// <summary>
/// Handed over to payroll. No amounts: settling here means the hours are agreed and passed on, not
/// that anybody has been paid.
/// </summary>
public sealed record TimeSheetSettled(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    UserSnapshot SettledBy,
    DateTimeOffset SettledAt
);

public sealed record TimeSheetCommented(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    TimeSheetComment Comment
);
