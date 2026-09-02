namespace HrAgencySystem.Api.Endpoints.Organization;

public static class Endpoint
{
    public static void Map(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("")
            .WithTags("Organization").WithOwnerRole();

        Maps.MapCreate.Map(group);
        Maps.MapUpdateSlug.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetBySlug.Map(group);
    }
}