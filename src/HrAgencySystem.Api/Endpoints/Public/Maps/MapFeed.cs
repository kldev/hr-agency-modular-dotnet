using HrAgencySystem.Files.Service;
using HrAgencySystem.Organization.Application.Port;

namespace HrAgencySystem.Api.Endpoints.Public.Maps;

internal static class MapFeed
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet(ApiEndpoints.Public.JobsXml, HandlerXml).WithSummary("Get feed jobs.xml");
        group.MapGet(ApiEndpoints.Public.JobsJson, HandlerJson).WithSummary("Get feed jobs.json");
    }

    private static Task<IResult> HandlerXml(
        IOrganizationSlugReservationRepository repository,
        IObjectStorage storage,
        string slug,
        CancellationToken ct
    ) => FeedFile.ServeAsync(repository, storage, slug, FeedFile.Xml, ct);

    private static Task<IResult> HandlerJson(
        IOrganizationSlugReservationRepository repository,
        IObjectStorage storage,
        string slug,
        CancellationToken ct
    ) => FeedFile.ServeAsync(repository, storage, slug, FeedFile.Json, ct);
}
