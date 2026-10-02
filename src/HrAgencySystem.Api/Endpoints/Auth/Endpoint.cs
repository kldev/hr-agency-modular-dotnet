using HrAgencySystem.Api.Endpoints.Auth.Maps;
using HrAgencySystem.Api.Infrastructure.OpenApi;

namespace HrAgencySystem.Api.Endpoints.Auth;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.Auth);
        MapLoginUser.Map(group);
        MapRefreshToken.Map(group);
        MapLogout.Map(group);
        MapRequestPasswordReset.Map(group);
        MapCompletePasswordReset.Map(group);
        MapLoginOwner.Map(group);
        MapCurrent.Map(group);
        MapCurrentOwner.Map(group);
        MapImpersonate.Map(group);
    }
}
