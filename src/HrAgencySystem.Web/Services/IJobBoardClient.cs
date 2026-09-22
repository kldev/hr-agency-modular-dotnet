namespace HrAgencySystem.Web.Services;

/// <summary>
/// Everything this host needs from the API, and nothing more. A null answer is "there is no such
/// board or post"; the API being down or refusing the key is <see cref="JobBoardUnavailableException"/>,
/// because the two mean different pages for the candidate - not found versus try again later.
/// </summary>
public interface IJobBoardClient
{
    Task<BoardInfo?> GetBoardAsync(string slug, CancellationToken ct);

    Task<BoardPost?> GetPostAsync(string slug, string postSlug, CancellationToken ct);

    Task<ApplyResult> ApplyAsync(
        string slug,
        string postSlug,
        BoardApplication application,
        CancellationToken ct
    );

    /// <summary>The feed file for the list page, or null when the board or its feed does not exist.</summary>
    Task<HttpResponseMessage?> GetFeedAsync(string slug, string format, CancellationToken ct);
}

public sealed record BoardInfo(string Slug, string Name);

/// <summary>
/// What the job page renders. The employment type is a string here on purpose: this host has no
/// reference to the domain, and the value is only ever printed.
/// </summary>
public sealed record BoardPost(
    string Title,
    string Description,
    string Location,
    string EmploymentType,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements
);

public sealed record BoardApplication(
    string FirstName,
    string LastName,
    string Email,
    string? Phone
);

public abstract record ApplyResult
{
    public sealed record Accepted : ApplyResult;

    /// <summary>The post is gone or closed since the page was opened.</summary>
    public sealed record PostNotFound : ApplyResult;

    /// <summary>The API refused the application and said why, in words a candidate can act on.</summary>
    public sealed record Rejected(IReadOnlyList<string> Reasons) : ApplyResult;
}

/// <summary>The API could not be reached, timed out, or did not accept this host's key.</summary>
public sealed class JobBoardUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
