using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

/// <summary>
/// The terms of a role change. <paramref name="NameChanged"/> is stated rather than derived,
/// because it is the one change other modules have to hear about: an assignment froze this
/// position's internal name when it was planned, and a rename has to reach it.
/// </summary>
public sealed record ProjectPositionUpdated(
    Guid ProjectId,
    Guid OrganizationId,
    ProjectPosition Position,
    bool NameChanged,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
