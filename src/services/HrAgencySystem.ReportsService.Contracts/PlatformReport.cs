namespace HrAgencySystem.ReportsService.Contracts;

/// <summary>Every organization on the platform, side by side, over a period.</summary>
public sealed record PlatformReport(
    string From,
    string To,
    PlatformTotals Totals,
    IReadOnlyList<OrganizationActivity> Organizations
);

public sealed record PlatformTotals(
    int Organizations,
    int ActiveOrganizations,
    int JobPostsPublished,
    int Applications,
    int InterviewsScheduled,
    int Hires,
    int ProjectsWentLive
);

/// <summary>
/// One organization. The counts are activity in the period; <see cref="ProjectsActive"/> is how many
/// projects are live right now, and <see cref="LastActivityAt"/> is the latest thing that happened
/// anywhere in the organization, whenever - a tenant quiet for the whole period still shows when it
/// was last seen.
/// </summary>
public sealed record OrganizationActivity(
    Guid OrganizationId,
    string Name,
    string Slug,
    DateTimeOffset CreatedAt,
    int JobPostsPublished,
    int Applications,
    int InterviewsScheduled,
    int Offers,
    int Hires,
    int ProjectsWentLive,
    int ProjectsActive,
    DateTimeOffset? LastActivityAt
);
