namespace HrAgencySystem.Projects.Contracts.IntegrationEvents;

/// <summary>
/// A role was renamed. Assignments froze the name when they were planned, so the frozen copy is
/// brought back into step - the id never moves, the name catches up.
/// <para>
/// The alternative, reading the name out of the owning module every time an assignment is shown,
/// would be a query across a module boundary on every row of a list. This is the cheaper half of
/// that trade, and it is why only the name travels: everything else on the role is read where it
/// lives.
/// </para>
/// </summary>
public sealed record ProjectPositionRenamed(
    Guid OrganizationId,
    Guid ProjectId,
    Guid PositionId,
    string Name,
    string ContractName
);
