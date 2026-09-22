using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HrAgencySystem.Web.Services;

/// <summary>
/// The job board's only way into the system: four routes under <c>/api/internal/boards</c>, the
/// service key in <c>X-Api-Key</c> (added by the <see cref="HttpClient"/> registration).
/// </summary>
public sealed class HttpJobBoardClient(HttpClient http, ILogger<HttpJobBoardClient> logger)
    : IJobBoardClient
{
    public const string ApiKeyHeader = "X-Api-Key";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public Task<BoardInfo?> GetBoardAsync(string slug, CancellationToken ct) =>
        GetAsync<BoardInfo>($"api/internal/boards/{Escape(slug)}", ct);

    public Task<BoardPost?> GetPostAsync(string slug, string postSlug, CancellationToken ct) =>
        GetAsync<BoardPost>(
            $"api/internal/boards/{Escape(slug)}/posts/{Escape(postSlug)}",
            ct
        );

    public async Task<ApplyResult> ApplyAsync(
        string slug,
        string postSlug,
        BoardApplication application,
        CancellationToken ct
    )
    {
        using var response = await SendAsync(
            () =>
                http.PostAsJsonAsync(
                    $"api/internal/boards/{Escape(slug)}/posts/{Escape(postSlug)}/applications",
                    application,
                    Json,
                    ct
                )
        );

        if (response.IsSuccessStatusCode)
            return new ApplyResult.Accepted();

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new ApplyResult.PostNotFound();

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return new ApplyResult.Rejected(await ReasonsAsync(response, ct));

        throw Unavailable(response);
    }

    public async Task<HttpResponseMessage?> GetFeedAsync(
        string slug,
        string format,
        CancellationToken ct
    )
    {
        var response = await SendAsync(
            () =>
                http.GetAsync(
                    $"api/internal/boards/{Escape(slug)}/feed.{format}",
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                )
        );

        if (response.IsSuccessStatusCode)
            return response;

        response.Dispose();

        return response.StatusCode == HttpStatusCode.NotFound ? null : throw Unavailable(response);
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
        where T : class
    {
        using var response = await SendAsync(() => http.GetAsync(url, ct));

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw Unavailable(response);

        return await response.Content.ReadFromJsonAsync<T>(Json, ct);
    }

    /// <summary>
    /// A network failure or a timeout becomes <see cref="JobBoardUnavailableException"/>, so a page
    /// has one thing to catch. A cancellation the caller asked for is left alone.
    /// </summary>
    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException exception)
        {
            throw new JobBoardUnavailableException("The API could not be reached.", exception);
        }
        catch (TaskCanceledException exception) when (exception.InnerException is TimeoutException)
        {
            throw new JobBoardUnavailableException("The API did not answer in time.", exception);
        }
        catch (Polly.Timeout.TimeoutRejectedException exception)
        {
            throw new JobBoardUnavailableException("The API did not answer in time.", exception);
        }
    }

    /// <summary>
    /// A 401 or 403 is this host's fault - a missing, wrong or revoked key - and is logged as such,
    /// because nothing the candidate does will fix it.
    /// </summary>
    private JobBoardUnavailableException Unavailable(HttpResponseMessage response)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            logger.LogError(
                "The API refused this host's service key ({Status}). Issue a new key and set InternalApi:ApiKey.",
                (int)response.StatusCode
            );
        else
            logger.LogWarning(
                "The API answered {Status} for {Url}",
                (int)response.StatusCode,
                response.RequestMessage?.RequestUri
            );

        return new JobBoardUnavailableException($"The API answered {(int)response.StatusCode}.");
    }

    /// <summary>The API's problem details, reduced to the sentences worth showing on the form.</summary>
    private static async Task<IReadOnlyList<string>> ReasonsAsync(
        HttpResponseMessage response,
        CancellationToken ct
    )
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<Problem>(Json, ct);

            if (problem?.ValidationErrors is { Count: > 0 } errors)
                return errors;

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
                return [problem.Detail];
        }
        catch (JsonException)
        {
            // Not problem details - fall through to the generic sentence.
        }

        return ["The application could not be accepted. Check the details and try again."];
    }

    private static string Escape(string value) => Uri.EscapeDataString(value);

    private sealed record Problem(string? Detail, IReadOnlyList<string>? ValidationErrors);
}
