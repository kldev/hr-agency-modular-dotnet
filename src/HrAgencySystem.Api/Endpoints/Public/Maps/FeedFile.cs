using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Feeds;
using HrAgencySystem.Files.Service;
using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;

namespace HrAgencySystem.Api.Endpoints.Public.Maps;

/// <summary>
/// An organization's generated feed file, found by its slug. Shared by the anonymous
/// <c>/p/{slug}/jobs.*</c> routes and the internal ones the job board reads through, so there is one
/// place that turns a slug into a storage key and the job board never learns the organization id.
/// </summary>
internal static class FeedFile
{
    public const string Xml = "xml";
    public const string Json = "json";

    public static async Task<IResult> ServeAsync(
        IOrganizationSlugReservationRepository organizations,
        IObjectStorage storage,
        string slug,
        string format,
        CancellationToken ct
    )
    {
        var (created, _) = OrganizationSlug.TryCreate(slug);

        var organization = created is null ? null : await organizations.FindBySlug(created, ct);

        if (organization == null)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        var result = await storage.GetAsync($"{organization.Value}/jobs.{format}", FeedBuckets.Jobs, ct);

        if (result.FileNotFound)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Feed", slug));

        return Results.File(
            result.OutputStream!,
            format == Xml ? "application/xml" : "application/json"
        );
    }
}
