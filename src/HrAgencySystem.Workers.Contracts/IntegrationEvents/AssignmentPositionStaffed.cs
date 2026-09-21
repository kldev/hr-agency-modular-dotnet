namespace HrAgencySystem.Workers.Contracts.IntegrationEvents;

/// <summary>
/// Somebody now takes up a seat on a project's role. Sent when an assignment is planned, which is
/// the moment the role stops being empty - a posting is staffed long before the person flies out.
/// <para>
/// The assignment's id travels with it because the other side keeps a set of ids rather than a
/// number: these messages arrive at least once, and a repeat has to be recognisable as one.
/// </para>
/// </summary>
public sealed record AssignmentPositionStaffed(
    Guid OrganizationId,
    Guid ProjectId,
    Guid PositionId,
    Guid AssignmentId
);
