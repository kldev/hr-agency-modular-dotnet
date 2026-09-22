using HrAgencySystem.Api.Infrastructure.OpenApi;
using HrAgencySystem.Api.Endpoints.Company.Maps;

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
