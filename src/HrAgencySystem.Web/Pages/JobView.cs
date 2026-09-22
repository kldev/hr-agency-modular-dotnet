using HrAgencySystem.Web.Services;

namespace HrAgencySystem.Web.Pages;

/// <summary>
/// What the public job page renders. Deliberately not the feed DTO - the feed is a separate
/// contract, and the two are free to drift.
/// </summary>
public sealed record JobView(
    string Title,
    string Description,
    string Location,
    string EmploymentType,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements
)
{
    public static JobView From(BoardPost post) =>
        new(
            post.Title,
            post.Description,
            post.Location,
            post.EmploymentType,
            post.Responsibilities,
            post.Requirements
        );
}
