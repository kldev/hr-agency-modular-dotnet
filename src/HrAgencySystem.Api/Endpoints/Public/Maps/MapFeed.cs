using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Files.Service;
using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;

namespace HrAgencySystem.Api.Endpoints.Public.Maps;

internal static class MapFeed
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{slug}/jobs.xml", HandlerXml).WithSummary("Get feed jobs.xml");
        group.MapGet("{slug}/jobs.json", HandlerJson).WithSummary("Get feed jobs.json");
    }

    private static async Task<IResult> HandlerXml(IOrganizationSlugReservationRepository repository,
        IFileStorage storage, string slug, CancellationToken ct)
    {
        var organization = await repository.FindBySlug(OrganizationSlug.Create(slug), ct);
        if (organization == null) return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        var result = await storage.GetAsync(organization.Value + "/jobs.xml", BucketNames.FeedJobs, ct);

        if (result.FileNotFound) return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        return Results.File(result.OutputStream!, "application/xml");

    }

    private static async Task<IResult> HandlerJson(IOrganizationSlugReservationRepository repository, string slug,  
        IFileStorage storage,CancellationToken ct)
    {
        var organization = await repository.FindBySlug(OrganizationSlug.Create(slug), ct);
        if (organization == null) return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));
        
        var result = await storage.GetAsync(organization.Value + "/jobs.json", BucketNames.FeedJobs, ct);

        if (result.FileNotFound) return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        return Results.File(result.OutputStream!, "application/json");
    }
}