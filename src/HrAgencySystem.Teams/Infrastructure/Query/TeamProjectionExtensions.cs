using HrAgencySystem.SharedKernel.Extensions;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Projections;

namespace HrAgencySystem.Teams.Infrastructure.Query;

internal static class TeamProjectionExtensions
{
    internal static IQueryable<TeamProjection> WithOrganizationId(
        this IQueryable<TeamProjection> query,
        OrganizationId organizationId
    )
    {
        return query.Where(t => t.OrganizationId == organizationId.Value);
    }

    internal static IQueryable<TeamProjection> WithSearch(
        this IQueryable<TeamProjection> query,
        string search
    )
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var normalized = search.Trim();

        return query.Where(t => t.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase));
    }

    internal static IQueryable<TeamProjection> WithMember(
        this IQueryable<TeamProjection> query,
        Guid? userId
    )
    {
        return userId.IsInvalid() ? query : query.Where(t => t.MemberIds.Contains(userId!.Value));
    }

    internal static IQueryable<TeamProjection> WithTeamId(
        this IQueryable<TeamProjection> query,
        Guid teamId
    )
    {
        return query.Where(t => t.Id == teamId);
    }
}
