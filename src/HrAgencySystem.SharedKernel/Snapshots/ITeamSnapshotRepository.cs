using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.SharedKernel.Snapshots;

public interface ITeamSnapshotRepository
{
    public const string NotFoundMessage = "The specified team does not exist in this organization.";

    /// <summary>
    /// Resolves a team only when it belongs to the given organization. There is no unscoped overload
    /// on purpose: every caller knows its tenant, and "does this id exist anywhere" is a question
    /// nobody here needs answered.
    /// </summary>
    Task<TeamSnapshot?> GetTeamAsync(
        Guid teamId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}

/// <summary>
/// Deliberately without the roster: a team exists precisely so that assignments survive rotation, so
/// freezing its members in somebody else's copy would defeat the point. The id is the durable
/// pointer, the composition is asked for when it is needed.
/// </summary>
public sealed record TeamSnapshot(Guid TeamId, string Name);
