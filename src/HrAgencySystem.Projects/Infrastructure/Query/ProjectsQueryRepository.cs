using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ProjectsQueryRepository(IQuerySession session) : IProjectsQueryRepository
{
    public async Task<SliceResponse<ProjectProjection>> GetProjects(
        OrganizationId organizationId,
        ProjectQuery query,
        CancellationToken ct
    )
    {
        var projects = session
            .Query<ProjectProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(query.Search)
            .WithFilters(query)
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id);

        return await projects.ToSlice(query, ct);
    }

    public async Task<ProjectProjection?> GetProject(
        OrganizationId organizationId,
        Guid projectId,
        CancellationToken ct
    ) =>
        await session
            .Query<ProjectProjection>()
            .WithOrganizationId(organizationId)
            .Where(p => p.Id == projectId)
            .FirstOrDefaultAsync(ct);
}
