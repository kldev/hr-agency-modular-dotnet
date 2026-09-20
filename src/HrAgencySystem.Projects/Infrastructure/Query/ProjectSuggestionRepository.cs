using HrAgencySystem.Projects.Application.Suggestion;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ProjectSuggestionRepository(IQuerySession session)
    : IProjectSuggestionRepository
{
    public async Task<IReadOnlyList<ProjectSuggestion>> Search(
        OrganizationId organizationId,
        string search,
        int limit,
        CancellationToken ct
    )
    {
        var projects = await session
            .Query<ProjectProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .OrderBy(p => p.Name)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(ct);

        return [.. projects.Select(p => p.ToSuggestion())];
    }
}
