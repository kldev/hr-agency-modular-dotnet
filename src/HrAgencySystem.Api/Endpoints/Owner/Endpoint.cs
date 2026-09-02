

namespace HrAgencySystem.Api.Endpoints.Owner;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Owner")
            .WithSummary("Owner").WithOwnerRole();
        
        Maps.MapCreate.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetAll.Map(group);
    }
}