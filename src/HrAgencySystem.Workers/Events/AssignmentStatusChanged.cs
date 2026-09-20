using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

/// <summary>
/// An assignment moves. <paramref name="EndsOn"/> travels with it because ending one is also
/// recording when it actually ended, which is rarely the day that was planned - and that date is
/// what the next posting's period is checked against.
/// </summary>
public sealed record AssignmentStatusChanged(
    Guid AssignmentId,
    Guid OrganizationId,
    Guid WorkerId,
    AssignmentStatus PreviousStatus,
    AssignmentStatus Status,
    DateOnly? EndsOn,
    string Reason,
    UserSnapshot ChangedBy,
    DateTimeOffset ChangedAt
);
