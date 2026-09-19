namespace HrAgencySystem.Api.Endpoints.Public;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(ApiEndpoints.Public.Group)
            .AllowAnonymous()
            .ExcludeFromDescription();
        Maps.MapFeed.Map(group);
    }
}
