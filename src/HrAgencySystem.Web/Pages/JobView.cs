using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Web.Pages;

/// <summary>
/// What the public job page renders. Deliberately not the feed DTO - the feed is a separate
/// contract with its own read model in <c>HrAgencySystem.Feeds</c>, and the two are free to drift.
/// </summary>
public sealed record JobView(
    string Title,
    string Description,
    string Location,
    EmploymentType EmploymentType,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements
)
{
    public static JobView FromProjection(JobPostProjection projection)
    {
        return new JobView(
            projection.Title,
            projection.Description,
            projection.Location,
            projection.EmploymentType,
            projection.Responsibilities,
            projection.Requirements
        );
    }
}
