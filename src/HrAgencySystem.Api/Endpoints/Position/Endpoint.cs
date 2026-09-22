using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.Position;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.EmploymentPositions);

        Maps.MapOpen.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapArchive.Map(group);
        Maps.MapRestore.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
    }
}
