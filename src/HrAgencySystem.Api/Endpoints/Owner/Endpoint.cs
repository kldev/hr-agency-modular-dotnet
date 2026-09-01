using HrAgencySystem.Api.Endpoints.Owner.Maps;

namespace HrAgencySystem.Api.Endpoints.Owner;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Owner")
            .WithSummary("Owner").WithOwnerRole();
        
        MapCreateOwner.Map(group);
        MapGetOwner.Map(group);
        MapGetOwners.Map(group);
    }
}