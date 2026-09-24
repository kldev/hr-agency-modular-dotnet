using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Tasks.Domain;

namespace HrAgencySystem.Tasks.Services;

public sealed class TasksService(
    IUserSnapshotRepository users,
    ICompanySnapshotRepository companies,
    IOpportunitySnapshotRepository opportunities,
    IOrganizationChecker checker
) : ITasksService
{
    public const string OpportunityNotFoundMessage = "The opportunity was not found.";

    public const string OpportunityOfAnotherCompanyMessage =
        "The opportunity belongs to another company than the task.";

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        if (!await checker.Exists(organizationId, ct))
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    public void ValidateAggregateUpdate(TaskItem? task, Guid commandOrganizationId, Guid taskId)
    {
        if (task is null || task.OrganizationId.Value != commandOrganizationId)
            throw new NotFoundException("Task", taskId);
    }

    public async Task<UserSnapshot> GetUserAsync(
        Guid userId,
        OrganizationId organizationId,
        CancellationToken ct
    ) =>
        await users.GetUserAsync(userId, organizationId, ct)
        ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);

    // A business rule rather than a 404: the caller named a company in a body, and must not learn
    // whether that id exists in somebody else's organization.
    public async Task<CompanySnapshot> GetCompanyAsync(
        Guid companyId,
        OrganizationId organizationId,
        CancellationToken ct
    ) =>
        await companies.GetCompanyAsync(companyId, organizationId, ct)
        ?? throw new BusinessRuleException(ICompanySnapshotRepository.NotFoundMessage);

    public async Task<TaskOpportunity> GetOpportunityAsync(
        Guid opportunityId,
        Guid companyId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var opportunity =
            await opportunities.GetOpportunityAsync(opportunityId, organizationId, ct)
            ?? throw new BusinessRuleException(OpportunityNotFoundMessage);

        if (opportunity.CompanyId != companyId)
            throw new BusinessRuleException(OpportunityOfAnotherCompanyMessage);

        return new TaskOpportunity(opportunity.OpportunityId, opportunity.Title);
    }
}
