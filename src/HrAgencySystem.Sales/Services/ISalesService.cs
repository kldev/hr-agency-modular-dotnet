using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Domain.FollowUp;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.FollowUp;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Sales.Services;

public interface ISalesService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
    Task<OpportunitySnapshot> GetOpportunityAsync(
        Guid organizationId,
        Guid opportunityId,
        CancellationToken ct
    );
    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
    Task<OrganizationId> GetBySlugAsync(string slug, CancellationToken ct);

    Task<FollowUpActionCreated> AppendFollowUpActionToStream(
        SalesOpportunityId opportunityId,
        OrganizationId organizationId,
        string content,
        DateTimeOffset followDateTime,
        UserSnapshot user,
        CancellationToken ct
    );

    Task<FollowUpActionUpdated> AppendFollowUpActionUpdateToStream(
        FollowUpActionId followUpActionId,
        OrganizationId organizationId,
        string content,
        DateTimeOffset followDateTime,
        UserSnapshot user,
        CancellationToken ct
    );
}
