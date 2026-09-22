using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.LegalEntity;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("")
            .WithSummary("Legal entities")
            .WithTags(ApiTags.AgencyLegalEntities);

        Maps.MapCreate.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapClose.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetSlice.Map(group);
    }
}
