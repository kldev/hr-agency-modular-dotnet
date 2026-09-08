namespace HrAgencySystem.Api.Endpoints.CompanyContacts;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/company-contacts").WithTags("Company contacts");
        Maps.MapCreate.Map(group);
        Maps.MapGetAll.Map(group);
        Maps.MapUpdate.Map(group);
    }
}