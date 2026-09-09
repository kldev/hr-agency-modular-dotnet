using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Extensions;

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
    
    
    internal static IQueryable<OpportunityProjection> WithResponsibleId(
        this IQueryable<OpportunityProjection> query, Guid? responsibleId)
    {
        return responsibleId.IsInvalid() ? query : query.Where(p => p.ResponsibleId == responsibleId);
    }
    
    internal static IQueryable<OpportunityProjection> WithOptionalCompanyId(
        this IQueryable<OpportunityProjection> query, Guid? companyId)
    {
        return companyId.IsInvalid() ? query : query.Where(p => p.CompanyId == companyId);
    }
    
    //
    
    internal static IQueryable<OpportunityProjection> WithStage(
        this IQueryable<OpportunityProjection> query, OpportunityStage? stage)
    {
        return !stage.HasValue ? query : query.Where(p => p.Stage == stage);
    }
    
    
    internal static IQueryable<OpportunityProjection> WithSearch(
        this IQueryable<OpportunityProjection> query, string search)
    {
        return string.IsNullOrEmpty(search) ? query : 
            query.Where(p => p.Company.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Where(p => p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Where(p => p.LostReason.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
}