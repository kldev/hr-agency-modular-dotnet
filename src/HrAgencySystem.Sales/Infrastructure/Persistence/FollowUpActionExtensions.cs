using HrAgencySystem.Sales.Documents;
using HrAgencySystem.SharedKernel.Extensions;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

internal static class FollowUpActionExtensions
{
    internal static IQueryable<FollowUpAction> WithOrganizationId(
        this IQueryable<FollowUpAction> query, Guid organizationId)
    {
        return query.Where(p => p.OrganizationId == organizationId);
    }

    internal static IQueryable<FollowUpAction> WithOpportunityId(
        this IQueryable<FollowUpAction> query, Guid? opportunityId)
    {
        return opportunityId.IsInvalid() ? query : query.Where(p => p.OpportunityId == opportunityId);
    }

    internal static IQueryable<FollowUpAction> WithCompanyId(
        this IQueryable<FollowUpAction> query, Guid? companyId)
    {
        return companyId.IsInvalid() ? query : query.Where(p => p.Company.Id == companyId);
    }
}
