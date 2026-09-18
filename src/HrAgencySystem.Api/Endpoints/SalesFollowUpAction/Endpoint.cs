namespace HrAgencySystem.Api.Endpoints.SalesFollowUpAction;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/sales/follow-up")
            .WithTags("Sales")
            .RequireAuthorization();

        Maps.MapCreate.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
    }
}
