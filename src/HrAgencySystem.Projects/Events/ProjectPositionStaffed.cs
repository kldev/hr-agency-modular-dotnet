namespace HrAgencySystem.Projects.Events;

/// <summary>
/// An assignment took up a seat on this role. Written onto the project's stream although the fact
/// comes from another module, so that it arrives after the event that opened the role and the
/// projection never has to fold a staffing onto a position it has not seen yet.
/// <para>
/// The aggregate has no <c>Apply</c> for it on purpose: who sits on a role is a read model
/// question, and nothing the project decides depends on the answer.
/// </para>
/// </summary>
public sealed record ProjectPositionStaffed(
    Guid ProjectId,
    Guid OrganizationId,
    Guid PositionId,
    Guid AssignmentId
);
