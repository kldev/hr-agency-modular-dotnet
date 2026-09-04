namespace HrAgencySystem.Api.Endpoints.Candidate;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/recruitment/candidates")
            .RequireAuthorization()
            .WithTags("Recruitment - Candidates");
        
        Maps.MapGet.Map(group);
        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapTag.Map(group);
        Maps.MapRemoveTag.Map(group);
    }
}