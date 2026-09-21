using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
// Public, not internal: Wolverine generates handler code into another assembly, and a handler that
// reaches this through IWorkersService cannot see an internal type - it falls back to service
// location and throws at runtime.
public sealed class PositionSnapshotRepository(IDocumentSession session)
    : IPositionSnapshotRepository
{
    public async Task<PositionSnapshot?> GetPositionAsync(
        Guid projectId,
        Guid positionId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var projection = await session
            .Query<ProjectPositionProjection>()
            .Where(p =>
                p.Id == positionId
                && p.ProjectId == projectId
                && p.OrganizationId == organizationId.Value
            )
            .FirstOrDefaultAsync(ct);

        if (projection is not null)
            return Describe(
                projection.Id,
                projection.ProjectId,
                projection.Name,
                projection.ContractName,
                projection.IsArchived
            );

        // The same fallback ProjectSnapshotRepository makes, and needed more here: a role is
        // typically opened seconds before somebody is planned onto it, and the daemon is behind.
        // Without this, the position would not exist yet at the one moment it is asked for.
        //
        // The role lives on the project's stream, which is why the project is a parameter: reading
        // it out of a second projection would only move the lag somewhere else.
        var replayed = await session.Events.AggregateStreamAsync<Project>(projectId, token: ct);

        if (replayed is null || replayed.OrganizationId.Value != organizationId.Value)
            return null;

        var found = replayed.PositionById(positionId);

        return found is null
            ? null
            : Describe(
                found.PositionId,
                replayed.Id.Value,
                found.Name,
                found.ContractName,
                found.IsArchived
            );
    }

    private static PositionSnapshot Describe(
        Guid id,
        Guid projectId,
        string name,
        string contractName,
        bool isArchived
    ) => new(id, projectId, name, contractName, isArchived);
}
