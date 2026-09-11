using HrAgencySystem.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapCurrentOwner
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/owner/me", Handler)
            .WithSummary("Get information about the current owner")
            .Produces<OwnerAuthenticated>()
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
    
    private static OwnerAuthenticated Handler(OwnerAuthenticated owner)
    {
        return owner;
    }
}