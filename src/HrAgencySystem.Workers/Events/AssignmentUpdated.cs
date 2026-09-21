using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

/// <summary>
/// The position or the period corrected. Not the project, not the person and not the engagement
/// type: changing any of those would make this a different posting wearing the old one's history,
/// and the answer to all three is a new assignment.
/// </summary>
public sealed record AssignmentUpdated(
    Guid AssignmentId,
    Guid OrganizationId,
    Guid WorkerId,
    AssignmentPosition Position,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
