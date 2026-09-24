using HrAgencySystem.Api.Infrastructure.OpenApi;

namespace HrAgencySystem.Api.Endpoints.CompanyContacts;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.CompanyContacts);
        Maps.MapCreate.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapDelete.Map(group);
    }
}
