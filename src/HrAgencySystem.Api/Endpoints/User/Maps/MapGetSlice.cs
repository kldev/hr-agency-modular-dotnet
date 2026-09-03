using HrAgencySystem.Api.Auth;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/users", Handler).WithSummary("Get users");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IUserQueryRepository repository,
        CancellationToken ct,
        [FromQuery] string? search,
        [FromQuery] OrganizationRole[] roles,
        int page = 1, int pageSize = 100)
    {
        var result = await repository
            .GetUsers(user.OrganizationId, search ?? "", roles ?? [], page, pageSize, ct);
        return TypedResults.Ok(result);
    }
}