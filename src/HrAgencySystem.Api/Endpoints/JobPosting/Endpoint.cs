using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.JobPosting;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("")
            .RequireAuthorization()
            .WithTags(ApiTags.RecruitmentJobPosting);
        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeRecruiter.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapApplyTo.Map(group);
        Maps.MapPostToChannel.Map(group);
        Maps.MapChangeStatus.Map(group);
    }
}
