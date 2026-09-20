using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Projects.Application.Port;

public interface IProjectsQueryRepository
{
    Task<SliceResponse<ProjectProjection>> GetProjects(
        OrganizationId organizationId,
        ProjectQuery query,
        CancellationToken ct
    );

    Task<ProjectProjection?> GetProject(
        OrganizationId organizationId,
        Guid projectId,
        CancellationToken ct
    );
}

public sealed record ProjectQuery(
    string Search,
    IReadOnlyList<ProjectStatus>? Statuses,
    Guid? CompanyId,
    Guid? TeamId,
    string? Country,
    int Page,
    int PageSize
) : IPagedQuery;
