using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PositionsQueryRepository(IQuerySession session) : IPositionsQueryRepository
{
    public async Task<SliceResponse<PositionListItem>> GetPositions(
        OrganizationId organizationId,
        PositionQuery query,
        CancellationToken ct
    )
    {
        var slice = await session
            .Query<ProjectPositionProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(query.Search)
            .WithFilters(query)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .ToSlice(query, ct);

        // The projects of this page only, read after paging rather than joined: both documents
        // belong to this module, and one extra query per page beats a name that goes stale.
        var projects = await LoadProjects(slice.Content.Select(p => p.ProjectId), ct);

        return new SliceResponse<PositionListItem>(
            [.. slice.Content.Select(position => ToListItem(position, Find(projects, position)))],
            slice.HasMore
        );
    }

    public async Task<PositionDetails?> GetPosition(
        OrganizationId organizationId,
        Guid positionId,
        CancellationToken ct
    )
    {
        var position = await session
            .Query<ProjectPositionProjection>()
            .WithOrganizationId(organizationId)
            .Where(p => p.Id == positionId)
            .FirstOrDefaultAsync(ct);

        if (position is null)
            return null;

        var project = await session.LoadAsync<ProjectProjection>(position.ProjectId, ct);

        return new PositionDetails(
            position,
            position.ProjectId,
            project?.Name ?? "",
            project?.CompanyName ?? "",
            project?.WorkCountry ?? "",
            position.WorkplaceAddress ?? project!.WorkplaceAddress
        );
    }

    private async Task<IReadOnlyList<ProjectProjection>> LoadProjects(
        IEnumerable<Guid> projectIds,
        CancellationToken ct
    )
    {
        var ids = projectIds.Distinct().ToArray();

        return ids.Length == 0 ? [] : await session.LoadManyAsync<ProjectProjection>(ct, ids);
    }

    private static ProjectProjection? Find(
        IReadOnlyList<ProjectProjection> projects,
        ProjectPositionProjection position
    ) => projects.FirstOrDefault(p => p.Id == position.ProjectId);

    private static PositionListItem ToListItem(
        ProjectPositionProjection position,
        ProjectProjection? project
    ) =>
        new(
            position.Id,
            position.ProjectId,
            project?.Name ?? "",
            project?.CompanyName ?? "",
            project?.WorkCountry ?? "",
            position.Name,
            position.ContractName,
            position.ContractType,
            position.ProposedRate,
            position.WeeklyHours,
            position.PlannedHeadcount,
            position.AssignedCount,
            position.MissingHeadcount,
            position.DefaultEngagementType,
            position.IsArchived,
            position.OpenedAt
        );
}
