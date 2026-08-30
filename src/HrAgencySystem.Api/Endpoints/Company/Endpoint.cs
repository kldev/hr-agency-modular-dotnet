using HrAgencySystem.Api.Endpoints.Company.Maps;

namespace HrAgencySystem.Api.Endpoints.Company;

public static class Endpoint
{
    public static void Map(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/companies").WithTags("Company");

        MapCreateCompany.Map(group);
        MapGetCompanies.Map(group);
    }
}