using HrAgencySystem.Api.Auth;
using HrAgencySystem.Identity.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapGetUsers
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/users", Handler);
    }

    private static async Task<IResult> Handler(AuthenticatedUser user, IDocumentSession session, CancellationToken ct)
    {
        var result = await session.Query<UserProjection>().Where(z=>z.OrganizationId == user.OrganizationId).ToListAsync(ct);
        return TypedResults.Ok(result);
    }
}