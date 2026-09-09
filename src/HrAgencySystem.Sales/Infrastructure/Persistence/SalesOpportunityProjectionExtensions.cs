using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Projections;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

internal static class SalesOpportunityProjectionExtensions
{
    internal static IQueryable<OpportunityProjection> WithOrganizationId(
        this IQueryable<OpportunityProjection> query, Guid organizationId)
    {
        return query.Where(p => p.OrganizationId == organizationId);
    }
    
    internal static IQueryable<OpportunityProjection> WithOpportunityId(
        this IQueryable<OpportunityProjection> query, Guid opportunityId)
    {
        return query.Where(p => p.Id == opportunityId);
    }
    
    internal static IQueryable<OpportunityProjection> WithCompanyId(
        this IQueryable<OpportunityProjection> query, Guid companyId)
    {
        return query.Where(p => p.CompanyId == companyId);
    }
    
    
    internal static IQueryable<OpportunityProjection> WithSalesOwnerId(
        this IQueryable<OpportunityProjection> query, Guid salesOwnerId)
    {
        return query.Where(p => p.ResponsibleId == salesOwnerId);
    }
}