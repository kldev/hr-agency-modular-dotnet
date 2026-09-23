using HrAgencySystem.Api.Infrastructure.OpenApi;

namespace HrAgencySystem.Api.Endpoints.FormResponses;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.FormsResponses);

        Maps.MapStart.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapSaveDraft.Map(group);
        Maps.MapSubmit.Map(group);
        Maps.MapCorrect.Map(group);
        Maps.MapForSubject.Map(group);
        Maps.MapAvailableForSubject.Map(group);
    }
}
