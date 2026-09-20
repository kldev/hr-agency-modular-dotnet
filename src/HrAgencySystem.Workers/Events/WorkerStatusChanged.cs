using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

/// <summary>
/// A person moves to the next stage, which also means they move to another department's list. The
/// stage they came from is carried so the history reads as a path rather than as a series of
/// unrelated states.
/// </summary>
public sealed record WorkerStatusChanged(
    Guid WorkerId,
    Guid OrganizationId,
    WorkerStatus PreviousStatus,
    WorkerStatus Status,
    string Reason,
    UserSnapshot ChangedBy,
    DateTimeOffset ChangedAt
);
