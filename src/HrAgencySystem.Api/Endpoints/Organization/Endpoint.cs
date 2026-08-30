using HrAgencySystem.Api.Endpoints.Organization.Maps;

namespace HrAgencySystem.Api.Endpoints.Organization;

public static class Endpoint
{
    public static void Map(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/organization").WithTags("Organization");

        MapCreateOrganization.Map(group);
        MapUpdateOrganizationSlug.Map(group);
    }
}