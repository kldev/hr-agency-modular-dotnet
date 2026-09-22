using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.Candidate;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("")
            .RequireAuthorization()
            .WithTags(ApiTags.RecruitmentCandidates);

        Maps.MapGet.Map(group);
        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapTag.Map(group);
        Maps.MapRemoveTag.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapTagList.Map(group);
        Maps.MapRemoveTagList.Map(group);
    }
}
