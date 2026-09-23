using HrAgencySystem.Api.Infrastructure.OpenApi;

namespace HrAgencySystem.Api.Endpoints.Forms;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.FormsDefinitions);

        Maps.MapGetSlice.Map(group);
        Maps.MapCreate.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapUpdateDetails.Map(group);
        Maps.MapSaveDraft.Map(group);
        Maps.MapPreviewLayout.Map(group);
        Maps.MapPublish.Map(group);
        Maps.MapArchive.Map(group);
        Maps.MapGetVersion.Map(group);
        Maps.MapFindResponses.Map(group);
    }
}
