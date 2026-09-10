using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Identity.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.Map("/api/users/{userId:guid}", Handler)
            .WithSummary("Get user")
            .ProducesStandardErrors();;
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IDocumentSession session, Guid userId, CancellationToken ct)
    {
        var result = await session.Query<UserProjection>()
            .Where(z => z.Id == userId && z.OrganizationId == user.OrganizationId).SingleOrDefaultAsync(ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("User", userId));
        }

        return TypedResults.Ok(result);
    }
}