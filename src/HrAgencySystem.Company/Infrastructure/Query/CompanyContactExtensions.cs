using HrAgencySystem.Company.Documents;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Company.Infrastructure.Query;

internal static class CompanyContactExtensions
{
    internal static IQueryable<CompanyContact> WithOrganizationId(
        this IQueryable<CompanyContact> query, OrganizationId organizationId)
    {
        return query.Where(z => z.OrganizationId == organizationId.Value);
    }

    internal static IQueryable<CompanyContact> WithCompanyId(
        this IQueryable<CompanyContact> query, CompanyId companyId)
    {
        return query.Where(z => z.CompanyId == companyId.Value);
    }


    internal static IQueryable<CompanyContact> WithCompanyId(
        this IQueryable<CompanyContact> query, Guid? companyId)
    {
        return !companyId.HasValue || companyId.Value == Guid.Empty
            ? query
            : query.Where(z => z.CompanyId == companyId.Value);
    }

    internal static IQueryable<CompanyContact> WithSearch(
        this IQueryable<CompanyContact> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return query;

        return query.Where(z =>
            z.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || z.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || z.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || z.JobTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || z.CompanyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }
}
