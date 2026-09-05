namespace HrAgencySystem.Api.Endpoints.JobPosting;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/recruitment/job-posting").RequireAuthorization().WithTags("Recruitment - Job Posting");
        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeRecruiter.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapApplyTo.Map(group);
        Maps.MapPostToChannel.Map(group);
    }
}