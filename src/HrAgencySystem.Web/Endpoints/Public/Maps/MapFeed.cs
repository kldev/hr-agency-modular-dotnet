using HrAgencySystem.Feeds;
using HrAgencySystem.Files.Service;
using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Web.Common.Errors;

namespace HrAgencySystem.Web.Endpoints.Public.Maps;

internal static class MapFeed
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{slug}/jobs.xml", HandlerXml).WithSummary("Get feed jobs.xml");
        group.MapGet("{slug}/jobs.json", HandlerJson).WithSummary("Get feed jobs.json");
    }

    private static async Task<IResult> HandlerXml(
        IOrganizationSlugReservationRepository repository,
        IObjectStorage storage,
        string slug,
        CancellationToken ct
    )
    {
        var organization = await repository.FindBySlug(OrganizationSlug.Create(slug), ct);
        if (organization == null)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        var result = await storage.GetAsync(organization.Value + "/jobs.xml", FeedBuckets.Jobs, ct);

        if (result.FileNotFound)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        return Results.File(result.OutputStream!, "application/xml");
    }

    private static async Task<IResult> HandlerJson(
        IOrganizationSlugReservationRepository repository,
        string slug,
        IObjectStorage storage,
        CancellationToken ct
    )
    {
        var organization = await repository.FindBySlug(OrganizationSlug.Create(slug), ct);
        if (organization == null)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        var result = await storage.GetAsync(
            organization.Value + "/jobs.json",
            FeedBuckets.Jobs,
            ct
        );

        if (result.FileNotFound)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        return Results.File(result.OutputStream!, "application/json");
    }
}
