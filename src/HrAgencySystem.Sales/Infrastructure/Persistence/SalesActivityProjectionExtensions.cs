using HrAgencySystem.Sales.Projections;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

internal static class SalesActivityProjectionExtensions
{
    internal static IQueryable<SalesActivityProjection> WithOrganizationId(
        this IQueryable<SalesActivityProjection> query, Guid organizationId)
    {
        return query.Where(p => p.OrgId == organizationId);
    }

    internal static IQueryable<SalesActivityProjection> WithCompanyId(
        this IQueryable<SalesActivityProjection> query, Guid? companyId)
    {

        return !companyId.HasValue || companyId == Guid.Empty
            ? query
            : query.Where(p => p.CompanyId == companyId);
    }

    internal static IQueryable<SalesActivityProjection> WithOpportunityId(
        this IQueryable<SalesActivityProjection> query, Guid? opportunityId)
    {

        return !opportunityId.HasValue || opportunityId == Guid.Empty
            ? query
            : query.Where(p => p.OpportunityId == opportunityId);
    }
}