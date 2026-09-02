using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;

namespace HrAgencySystem.Identity.Infrastructure.Query;

internal static class UserSuggestionsExtensions
{
    public static IQueryable<UserProjection> WithOrganizationId(this IQueryable<UserProjection> query,
        Guid organizationId)
    {
        return query.Where(u => u.OrganizationId == organizationId);
    }

    public static IQueryable<UserProjection> WithSearch(this IQueryable<UserProjection> query, string search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;

        var querySearch = $"%{search}%".ToLower();
        return query.Where(u => u.Email.ToLower().Contains(querySearch) 
                                || u.FirstName.Contains(querySearch) 
                                || u.LastName.Contains(querySearch));
    }

    public static IQueryable<UserProjection> WithRoles(this IQueryable<UserProjection> query, IReadOnlyList<OrganizationRole> roles)
    {
        return roles.Count == 0 ? query : query.Where(u => roles.Contains(u.Role));
    }
}