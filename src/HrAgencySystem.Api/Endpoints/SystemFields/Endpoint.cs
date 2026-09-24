using HrAgencySystem.Api.Infrastructure.OpenApi;

namespace HrAgencySystem.Api.Endpoints.SystemFields;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.FormsSystemFields);

        Maps.MapList.Map(group);
        Maps.MapDefine.Map(group);
        Maps.MapAddStandard.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapArchive.Map(group);
    }
}
