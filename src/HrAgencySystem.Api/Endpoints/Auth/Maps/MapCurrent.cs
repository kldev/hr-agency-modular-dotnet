using HrAgencySystem.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapCurrent
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/user/me", Handler)
            .WithSummary("Get information about the current user")
            .Produces<AppUserAuthenticated>()
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
    
    private static AppUserAuthenticated Handler(AppUserAuthenticated user)
    {
        return user;
    }
}

