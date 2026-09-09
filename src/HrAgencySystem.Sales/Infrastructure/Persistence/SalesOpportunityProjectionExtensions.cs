using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Projections;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

internal static class SalesOpportunityProjectionExtensions
{
    internal static IQueryable<SalesOpportunityProjection> WithOrganizationId(
        this IQueryable<SalesOpportunityProjection> query, Guid organizationId)
    {
        return query.Where(p => p.OrganizationId == organizationId);
    }
    
    internal static IQueryable<SalesOpportunityProjection> WithOpportunityId(
        this IQueryable<SalesOpportunityProjection> query, Guid opportunityId)
    {
        return query.Where(p => p.Id == opportunityId);
    }
    
    internal static IQueryable<SalesOpportunityProjection> WithCompanyId(
        this IQueryable<SalesOpportunityProjection> query, Guid companyId)
    {
        return query.Where(p => p.CompanyId == companyId);
    }
    
    
    internal static IQueryable<SalesOpportunityProjection> WithSalesOwnerId(
        this IQueryable<SalesOpportunityProjection> query, Guid salesOwnerId)
    {
        return query.Where(p => p.SalesOwnerId == salesOwnerId);
    }
}