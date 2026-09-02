using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Identity.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.Map("/api/users/{userId:guid}", Handler)
            .WithSummary("Get user")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IDocumentSession session, Guid userId, CancellationToken ct)
    {
        var result = await session.Query<UserProjection>()
            .Where(z => z.Id == userId && z.OrganizationId == user.OrganizationId).SingleOrDefaultAsync(ct);

        if (result == null)
            return TypedResults.NotFound(new ProblemDetails()
            {
                Title = "User not found",
                Status = StatusCodes.Status404NotFound, Detail = $"User with {userId} id not found"
            });

        return TypedResults.Ok(result);
    }
}