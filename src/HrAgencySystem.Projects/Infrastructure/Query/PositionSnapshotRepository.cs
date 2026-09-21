using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PositionSnapshotRepository(IDocumentSession session)
    : IPositionSnapshotRepository
{
    public async Task<PositionSnapshot?> GetPositionAsync(
        Guid positionId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var projection = await session
            .Query<ProjectPositionProjection>()
            .Where(p => p.Id == positionId && p.OrganizationId == organizationId.Value)
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
        // Without this, the position would simply not exist yet at the one moment it is asked for.
        //
        // The position lives on the project's stream, so finding it means replaying the project -
        // and the only way in is the project id, which this method does not have. Hence the query
        // by position id below: it reads the projection of the project that owns it.
        var project = await session
            .Query<ProjectProjection>()
            .Where(p => p.OrganizationId == organizationId.Value)
            .Where(p => p.Positions.Any(position => position.PositionId == positionId))
            .FirstOrDefaultAsync(ct);

        if (project is null)
            return null;

        var replayed = await session.Events.AggregateStreamAsync<Project>(project.Id, token: ct);
        var found = replayed?.PositionById(positionId);

        return found is null
            ? null
            : Describe(
                found.PositionId,
                replayed!.Id.Value,
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
