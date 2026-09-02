namespace HrAgencySystem.Api.Endpoints.User;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithSummary("Users").WithTags("Users");
        
        Maps.MapCreate.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetAll.Map(group);
    }
}