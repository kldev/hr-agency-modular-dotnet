using HrAgencySystem.Api.Endpoints.Public.Maps;
using HrAgencySystem.Files.Service;
using HrAgencySystem.Organization.Application.Port;

namespace HrAgencySystem.Api.Endpoints.Internal.Maps;

/// <summary>
/// The feed the board's list page is built from. Passed through rather than redirected to the
/// anonymous route: the page fetches it from its own origin, and a redirect would make that a
/// cross-origin request.
/// </summary>
internal static class MapGetFeed
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet(ApiEndpoints.Internal.FeedJson, HandlerJson).WithSummary("The board's feed, json");
        group.MapGet(ApiEndpoints.Internal.FeedXml, HandlerXml).WithSummary("The board's feed, xml");
    }

    private static Task<IResult> HandlerJson(
        IOrganizationSlugReservationRepository organizations,
        IObjectStorage storage,
        string slug,
        CancellationToken ct
    ) => FeedFile.ServeAsync(organizations, storage, slug, FeedFile.Json, ct);

    private static Task<IResult> HandlerXml(
        IOrganizationSlugReservationRepository organizations,
        IObjectStorage storage,
        string slug,
        CancellationToken ct
    ) => FeedFile.ServeAsync(organizations, storage, slug, FeedFile.Xml, ct);
}
