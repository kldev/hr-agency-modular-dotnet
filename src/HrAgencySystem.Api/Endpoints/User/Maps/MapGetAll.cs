using HrAgencySystem.Api.Auth;
using HrAgencySystem.Identity.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapGetAll
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/users", Handler);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IDocumentSession session,CancellationToken ct, string? search,
        int page = 1, int pageSize = 100)
    {
        var normalizePage = page < 1 ? 1 : page;
        var normalizePageSize = Math.Clamp(pageSize, 20, 500);
        var result = await session.Query<UserProjection>()
            .Where(z=>z.OrganizationId == user.OrganizationId)
            .Take(pageSize).Skip((normalizePage-1)*normalizePageSize).ToListAsync(ct);
        return TypedResults.Ok(result);
    }
}