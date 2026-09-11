using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Extensions;

namespace HrAgencySystem.Identity.Infrastructure.Query;

internal static class UserProjectionExtensions
{
    internal static IQueryable<UserProjection> WithOrganizationId(this IQueryable<UserProjection> query,
        Guid organizationId)
    {
        return query.Where(u => u.OrganizationId == organizationId);
    }
    
    internal static IQueryable<UserProjection> WithOptionalOrganizationId(this IQueryable<UserProjection> query,
        Guid? organizationId)
    {
        return organizationId.IsInvalid() ? query : query.Where(u => u.OrganizationId == organizationId);
    }

    internal static IQueryable<UserProjection> WithSearch(this IQueryable<UserProjection> query, string search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;

        var querySearch = search.Trim();
        return query.Where(u => u.Email.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || u.FirstName.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || u.LastName.Contains(querySearch, StringComparison.OrdinalIgnoreCase));
    }

    internal static IQueryable<UserProjection> WithRoles(this IQueryable<UserProjection> query, IReadOnlyList<OrganizationRole> roles)
    {
        return roles.Count == 0 ? query : query.Where(u => roles.Contains(u.Role));
    }
    
    internal static IQueryable<UserProjection> WithUserId(this IQueryable<UserProjection> query,
        Guid userId)
    {
        return query.Where(u => u.Id == userId);
    }

    internal static IQueryable<UserProjection> WithoutSystemRole(this IQueryable<UserProjection> query)
    {
        return query.Where(q => q.Role != OrganizationRole.System);
    }
}