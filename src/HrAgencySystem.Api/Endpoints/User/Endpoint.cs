using HrAgencySystem.Api.Endpoints.User.Maps;

namespace HrAgencySystem.Api.Endpoints.User;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithSummary("Users").WithTags("Users");
        
        MapCreateUser.Map(group);
        MapGetUser.Map(group);
        MapGetUsers.Map(group);
    }
}