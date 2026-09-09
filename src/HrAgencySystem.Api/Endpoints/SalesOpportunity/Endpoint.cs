namespace HrAgencySystem.Api.Endpoints.SalesOpportunity;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sales/opportunity")
            .WithTags("Sales")
            .RequireAuthorization();
        
        Maps.MapCreate.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeResponsible.Map(group);
    }
}