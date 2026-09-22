using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.Company;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.Company);

        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetByTaxId.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapCompleteProfile.Map(group);
        Maps.MapGetContacts.Map(group);
    }
}
