using HrAgencySystem.Organization.Projections;

namespace HrAgencySystem.Organization.Infrastructure.Persistence;

internal static class OrganizationProjectionExtensions
{
    internal static IQueryable<OrganizationProjection> WithSearch(this IQueryable<OrganizationProjection> query, string? search)
    {
        return string.IsNullOrEmpty(search)
            ? query
            : query.Where(x =>
                x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                || x.Slug.Contains(search, StringComparison.InvariantCultureIgnoreCase));
    }
}