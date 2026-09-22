using HrAgencySystem.Web.Services;

namespace HrAgencySystem.Web.Endpoints.Public.Maps;

/// <summary>
/// The feed the list page loads with <c>fetch("/{slug}/jobs.json")</c>, passed through from the
/// API. Served here rather than redirected: the page asks its own origin, and a redirect would turn
/// that into a cross-origin request.
/// </summary>
internal static class MapFeed
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{slug}/jobs.xml", (IJobBoardClient board, HttpContext http, string slug, CancellationToken ct) =>
            Serve(board, http, slug, "xml", ct)).WithSummary("Get feed jobs.xml");

        group.MapGet("{slug}/jobs.json", (IJobBoardClient board, HttpContext http, string slug, CancellationToken ct) =>
            Serve(board, http, slug, "json", ct)).WithSummary("Get feed jobs.json");
    }

    private static async Task<IResult> Serve(
        IJobBoardClient board,
        HttpContext http,
        string slug,
        string format,
        CancellationToken ct
    )
    {
        HttpResponseMessage? feed;

        try
        {
            feed = await board.GetFeedAsync(slug, format, ct);
        }
        catch (JobBoardUnavailableException)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "The job feed is unavailable right now."
            );
        }

        if (feed is null)
            return TypedResults.NotFound();

        // The response owns the stream being copied out; it goes when the request does.
        http.Response.RegisterForDispose(feed);

        return Results.Stream(
            await feed.Content.ReadAsStreamAsync(ct),
            feed.Content.Headers.ContentType?.ToString()
                ?? (format == "xml" ? "application/xml" : "application/json")
        );
    }
}
