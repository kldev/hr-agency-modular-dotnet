using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Tasks.Domain;

namespace HrAgencySystem.Tasks.Services;

/// <summary>
/// The module's one way out. A task knows a company, a deal and people; all three come through
/// SharedKernel ports, always asked within the caller's organization.
/// </summary>
public interface ITasksService
{
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    /// <summary>A 404 for somebody else's task - a 403 would confirm the id exists elsewhere.</summary>
    void ValidateAggregateUpdate(TaskItem? task, Guid commandOrganizationId, Guid taskId);

    Task<UserSnapshot> GetUserAsync(Guid userId, OrganizationId organizationId, CancellationToken ct);

    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, OrganizationId organizationId, CancellationToken ct);

    /// <summary>The deal, checked to be the same company's as the task.</summary>
    Task<TaskOpportunity> GetOpportunityAsync(
        Guid opportunityId,
        Guid companyId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}
