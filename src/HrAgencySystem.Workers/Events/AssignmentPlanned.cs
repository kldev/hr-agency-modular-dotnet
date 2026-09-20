using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

/// <summary>
/// One person put on one project for one period.
/// <para>
/// <paramref name="WorkerFullName"/> is the only thing about the person carried here, and it is
/// carried because a register of postings that cannot say who was posted is not a register. Nothing
/// else about them travels: no birth date, no document number, no address. Those stay on the file.
/// </para>
/// </summary>
public sealed record AssignmentPlanned(
    Guid AssignmentId,
    Guid OrganizationId,
    Guid WorkerId,
    string WorkerFullName,
    ProjectPlacementSnapshot Project,
    EngagementType EngagementType,
    string Position,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);
