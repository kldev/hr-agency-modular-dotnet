using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

/// <summary>
/// A role is closed. Archived rather than deleted: at twenty-five positions on a large client half
/// of them are roles that have run their course, and hiding them from the picker is the point -
/// but every assignment ever planned onto one still names it.
/// </summary>
public sealed record ProjectPositionArchived(
    Guid ProjectId,
    Guid OrganizationId,
    Guid PositionId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
