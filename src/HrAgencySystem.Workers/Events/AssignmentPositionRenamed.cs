using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

/// <summary>
/// The role this posting is held against was renamed in the project it belongs to, and the copy
/// frozen here follows. Only the name: the id is what everything is keyed on and it never moves.
/// <para>
/// This is written per assignment rather than resolved on read, so the register stays answerable
/// from its own documents - see <see cref="AssignmentPosition"/> for why the copy exists at all.
/// </para>
/// </summary>
public sealed record AssignmentPositionRenamed(
    Guid AssignmentId,
    Guid OrganizationId,
    /// The person's own row carries a copy of this name too, so the event has to reach them.
    Guid WorkerId,
    Guid PositionId,
    string Name,
    string ContractName
);
