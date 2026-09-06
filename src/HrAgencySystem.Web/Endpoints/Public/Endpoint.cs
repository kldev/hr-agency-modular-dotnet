namespace HrAgencySystem.Web.Endpoints.Public;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").AllowAnonymous().ExcludeFromDescription();
        Maps.MapFeed.Map(group);
    }
}