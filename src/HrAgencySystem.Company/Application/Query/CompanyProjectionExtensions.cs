using HrAgencySystem.Company.Projections;

namespace HrAgencySystem.Company.Application.Query;

internal static class CompanyProjectionExtensions
{

    internal static IQueryable<CompanyProjection> WithOrganizationId(this IQueryable<CompanyProjection> query,
        Guid organizationId)
    {
        return query.Where(q => q.OrganizationId == organizationId);
    }

    internal static IQueryable<CompanyProjection> WithCompanyId(this IQueryable<CompanyProjection> query,
        Guid? companyId)
    {
        return companyId.HasValue ? query.Where(q => q.Id == companyId) : query;
    }

    internal static IQueryable<CompanyProjection> WithTax(this IQueryable<CompanyProjection> query, string? taxId)
    {
        return string.IsNullOrWhiteSpace(taxId)
            ? query
            : query.Where(q =>
                q.TaxId.Contains(taxId ?? "", StringComparison.OrdinalIgnoreCase));

    }

    internal static IQueryable<CompanyProjection> WithSearch(this IQueryable<CompanyProjection> query,
        string? search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(q =>
                q.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                q.TaxId.Contains(search, StringComparison.OrdinalIgnoreCase));

    }
}