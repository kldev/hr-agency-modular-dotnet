using HrAgencySystem.Api.Infrastructure.OpenApi;

namespace HrAgencySystem.Api.Endpoints.Tasks;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.SalesTasks).RequireAuthorization();

        Maps.MapGetBoard.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapCreate.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapComplete.Map(group);
        Maps.MapReopen.Map(group);
    }
}
