namespace HrAgencySystem.Api.Endpoints.Public;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("p").AllowAnonymous().ExcludeFromDescription();
        Maps.MapFeed.Map(group);
    }
}