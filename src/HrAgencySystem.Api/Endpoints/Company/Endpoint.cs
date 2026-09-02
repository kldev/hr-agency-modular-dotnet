namespace HrAgencySystem.Api.Endpoints.Company;

public static class Endpoint
{
    public static void Map(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Company");

        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetByTaxId.Map(group);
    }
}