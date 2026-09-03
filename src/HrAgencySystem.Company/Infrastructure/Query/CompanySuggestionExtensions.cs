using HrAgencySystem.Company.Projections;

namespace HrAgencySystem.Company.Infrastructure.Query;

internal static class CompanySuggestionExtensions
{
    public static IQueryable<CompanyProjection> WithOrganizationId(this IQueryable<CompanyProjection> query,
        Guid organizationId)
    {
        return query.Where(z=>z.OrganizationId == organizationId);
    }

    public static IQueryable<CompanyProjection> WithSearch(this IQueryable<CompanyProjection> query, string search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(z =>
                z.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                z.TaxId.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    public static IQueryable<CompanyProjection> WithCountryCode(this IQueryable<CompanyProjection> query, string countryCode)
    {
        return string.IsNullOrWhiteSpace(countryCode) ? query : 
            query.Where(z => z.CountryCode.Contains( countryCode, StringComparison.OrdinalIgnoreCase));
    }
}