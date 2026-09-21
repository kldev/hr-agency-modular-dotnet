using HrAgencySystem.Agency.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Agency.Events;

/*
 * Every event names the organization because the organization *is* the stream: the whole chart is
 * one aggregate. That is what lets "exactly one root", "no unit under its own descendant" and
 * "one person, one unit" be settled inside a single transaction instead of against a read model
 * that is always a moment behind. The same trade the positions took inside a project.
 */

public sealed record OrgUnitCreated(
    Guid OrganizationId,
    Guid UnitId,
    Guid? ParentId,
    string Name,
    OrgUnitKind Kind,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);

public sealed record OrgUnitRenamed(
    Guid OrganizationId,
    Guid UnitId,
    string Name,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

/// <summary>A unit changes parent. Its people and its head travel with it; only the box moves.</summary>
public sealed record OrgUnitMoved(
    Guid OrganizationId,
    Guid UnitId,
    Guid ParentId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

public sealed record OrgUnitHeadAssigned(
    Guid OrganizationId,
    Guid UnitId,
    Guid HeadUserId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

/// <summary>
/// The unit is left without a head of its own, which is a normal state: whoever heads the unit
/// above answers for it until somebody is named.
/// </summary>
public sealed record OrgUnitHeadCleared(
    Guid OrganizationId,
    Guid UnitId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

public sealed record OrgUnitMemberAdded(
    Guid OrganizationId,
    Guid UnitId,
    OrgUnitMember Member,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

public sealed record OrgUnitMemberRemoved(
    Guid OrganizationId,
    Guid UnitId,
    Guid UserId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

/// <summary>
/// Archived rather than deleted: a department that was dissolved still stands next to the leave
/// somebody approved in it last year.
/// </summary>
public sealed record OrgUnitArchived(
    Guid OrganizationId,
    Guid UnitId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
