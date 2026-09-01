using HrAgencySystem.Api.Endpoints.Auth.Maps;

namespace HrAgencySystem.Api.Endpoints.Auth;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Auth");
        MapLoginUser.Map(group);
        MapLoginOwner.Map(group);
    }
}