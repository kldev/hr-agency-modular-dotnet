namespace HrAgencySystem.Api.Endpoints.Sales;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Sales").RequireAuthorization();

        Maps.MapLogActivity.Map(group);
        Maps.MapGetActivitySlice.Map(group);
    }
}
