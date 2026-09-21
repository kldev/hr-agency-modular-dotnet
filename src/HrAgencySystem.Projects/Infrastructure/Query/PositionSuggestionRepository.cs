using HrAgencySystem.Projects.Application.Suggestion;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PositionSuggestionRepository(IQuerySession session)
    : IPositionSuggestionRepository
{
    public async Task<IReadOnlyList<PositionSuggestion>> Search(
        OrganizationId organizationId,
        Guid? projectId,
        string search,
        int limit,
        CancellationToken ct
    )
    {
        var positions = session
            .Query<ProjectPositionProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .Where(p => !p.IsArchived);

        if (projectId is not null)
            positions = positions.Where(p => p.ProjectId == projectId);

        var found = await positions
            .OrderBy(p => p.Name)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(ct);

        return [.. found.Select(ToSuggestion)];
    }

    public async Task<PositionSuggestion?> ById(
        OrganizationId organizationId,
        Guid positionId,
        CancellationToken ct
    )
    {
        // Answered even when archived: a picker showing what was chosen before the role closed
        // still has to name it.
        var position = await session
            .Query<ProjectPositionProjection>()
            .WithOrganizationId(organizationId)
            .FirstOrDefaultAsync(p => p.Id == positionId, ct);

        return position is null ? null : ToSuggestion(position);
    }

    private static PositionSuggestion ToSuggestion(ProjectPositionProjection position) =>
        new(
            position.Id,
            position.ProjectId,
            position.Name,
            position.ContractName,
            position.ContractType,
            position.DefaultEngagementType,
            position.PlannedHeadcount,
            position.AssignedCount
        );
}
