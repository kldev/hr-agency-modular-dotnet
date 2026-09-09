using HrAgencySystem.Sales.Projections;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

internal static class SalesActivityProjectionExtensions
{
    internal static IQueryable<ActivityProjection> WithOrganizationId(
        this IQueryable<ActivityProjection> query, Guid organizationId)
    {
        return query.Where(p => p.OrgId == organizationId);
    }

    internal static IQueryable<ActivityProjection> WithCompanyId(
        this IQueryable<ActivityProjection> query, Guid? companyId)
    {

        return !companyId.HasValue || companyId == Guid.Empty
            ? query
            : query.Where(p => p.CompanyId == companyId);
    }

    internal static IQueryable<ActivityProjection> WithOpportunityId(
        this IQueryable<ActivityProjection> query, Guid? opportunityId)
    {

        return !opportunityId.HasValue || opportunityId == Guid.Empty
            ? query
            : query.Where(p => p.OpportunityId == opportunityId);
    }
}