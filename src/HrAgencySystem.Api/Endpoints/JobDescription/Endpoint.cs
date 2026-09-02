namespace HrAgencySystem.Api.Endpoints.JobDescription;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Job Description").RequireAuthorization();

        Maps.MapCreate.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapAssignRecruiter.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGetStatusHistory.Map(group);
        Maps.MapUpdateStatus.Map(group);
    }
}