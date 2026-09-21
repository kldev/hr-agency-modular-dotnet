using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// The role somebody was put on, as it was named when they were put on it.
/// <para>
/// A snapshot for the same reason <see cref="ProjectPlacementSnapshot"/> is one, with one
/// difference worth stating: this one is kept in step. A role renamed in the project sends an
/// integration event and the name here follows, because a register whose rows say "Painter" while
/// the project calls the role "Painter Belgium" is a register nobody trusts. What does not follow
/// is the id: that never changes, and it is what everything else is keyed on.
/// </para>
/// <para>
/// <paramref name="ContractName"/> rides along so a document can be made from the assignment alone.
/// If the role's wording changes before the next contract is printed, the position is the authority
/// and this is the fallback.
/// </para>
/// </summary>
public sealed record AssignmentPosition(Guid PositionId, string Name, string ContractName)
{
    public static AssignmentPosition From(PositionSnapshot snapshot) =>
        new(snapshot.Id, snapshot.Name, snapshot.ContractName);
}
