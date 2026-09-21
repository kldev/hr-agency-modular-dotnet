using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.SharedKernel.Snapshots;

/// <summary>
/// A role opened inside a project, as seen by a module that does not own projects. Read by the
/// workers module, which puts a named person on one.
/// </summary>
public interface IPositionSnapshotRepository
{
    public const string NotFoundMessage = "Required position data not found.";

    /// <summary>
    /// Resolves a position only when it belongs to the given organization, for the same reason a
    /// project is resolved that way: nobody is ever assigned to another agency's role.
    /// </summary>
    Task<PositionSnapshot?> GetPositionAsync(
        Guid positionId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}

/// <summary>
/// What an assignment needs to know about the role it is held against.
/// <para>
/// <paramref name="Name"/> is the internal one - "Painter Belgium", the one that tells two roles
/// apart. The name that goes on a document is read from the position itself at the moment the
/// document is made, because that is a different question asked at a different time.
/// </para>
/// <para>
/// Deliberately thin. Everything else a role says - the rate, the hours, the duties - is the
/// position's business until somebody signs a contract for it, and then it belongs to that
/// contract rather than to this snapshot.
/// </para>
/// </summary>
public sealed record PositionSnapshot(
    Guid Id,
    Guid ProjectId,
    string Name,
    string ContractName,
    bool IsArchived
)
{
    /// <summary>
    /// An archived role takes no new people. The ones already on it stay: a role ends while its
    /// people work out their notice.
    /// </summary>
    public bool IsOpenForAssignments => !IsArchived;

    /// <summary>
    /// Whether this role belongs to the delivery somebody is being assigned to. Asked because the
    /// assignment names both, and two ids that disagree mean one of them was picked by mistake.
    /// </summary>
    public bool BelongsTo(Guid projectId) => ProjectId == projectId;
}
