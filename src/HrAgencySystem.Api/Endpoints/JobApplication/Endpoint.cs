namespace HrAgencySystem.Api.Endpoints.JobApplication;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/recruitment/job-applications")
            .RequireAuthorization().WithTags("Recruitment - Job Applications");
        
        Maps.MapGet.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapTag.Map(group);
        Maps.MapRemoveTag.Map(group);
    }
}