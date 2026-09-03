namespace HrAgencySystem.Api.Endpoints.JobPosting;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").RequireAuthorization().WithTags("Recruitment - Job Posting");
        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
    }
}