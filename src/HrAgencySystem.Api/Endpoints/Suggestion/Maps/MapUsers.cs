using HrAgencySystem.Api.Auth;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapUsers
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group.MapGet("/api/suggestion/users", Handler).WithSummary("Get top 25 users");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,IUserSuggestionRepository repository,
        [FromQuery] string? search, [FromQuery] OrganizationRole[] roles,   CancellationToken ct)
    {
        var result = await repository.GetUserSuggestions(user.OrganizationId, search ?? "", roles, ct);
        return TypedResults.Ok(result);
    }
}