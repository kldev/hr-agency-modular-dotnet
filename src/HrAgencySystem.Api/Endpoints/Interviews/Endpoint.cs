namespace HrAgencySystem.Api.Endpoints.Interviews;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/interviews")
            .RequireAuthorization().WithTags("Recruitment - Interviews");
        
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapSchedule.Map(group);
        Maps.MapChangeFormat.Map(group);
        Maps.MapChangeStatus.Map(group);
    }
}