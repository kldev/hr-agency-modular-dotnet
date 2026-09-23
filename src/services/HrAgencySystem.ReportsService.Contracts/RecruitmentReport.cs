namespace HrAgencySystem.ReportsService.Contracts;

/// <summary>
/// One organization's recruitment over a period.
/// <para>
/// Two kinds of numbers, deliberately kept apart. <see cref="Funnel"/> follows a <b>cohort</b>: the
/// applications received in the period and how far each of them has got, whenever that happened.
/// <see cref="Months"/> and the activity figures in <see cref="Totals"/> count <b>what happened</b>
/// in each month - an offer made in May for a March application is May's offer. Mixing the two is
/// how a funnel ends up with more hires than applications.
/// </para>
/// </summary>
public sealed record RecruitmentReport(
    string From,
    string To,
    RecruitmentTotals Totals,
    RecruitmentFunnel Funnel,
    IReadOnlyList<RecruitmentMonth> Months,
    IReadOnlyList<SourceCount> Sources
);

/// <summary>Activity in the period.</summary>
public sealed record RecruitmentTotals(
    int JobPostsPublished,
    int Applications,
    int InterviewsScheduled,
    int InterviewsHeld,
    int Offers,
    int Hires
);

/// <summary>
/// The applications received in the period, by the furthest stage each has reached.
/// <see cref="HireRate"/> is hires over applications, 0-1, and null when there were none.
/// </summary>
public sealed record RecruitmentFunnel(
    int Applied,
    int Screening,
    int Interview,
    int Assessment,
    int Offer,
    int Hired,
    int Rejected,
    int Withdrawn,
    decimal? HireRate
);

/// <summary>What happened in one month; <see cref="Month"/> is yyyy-MM.</summary>
public sealed record RecruitmentMonth(
    string Month,
    int Applications,
    int InterviewsScheduled,
    int Offers,
    int Hires
);

/// <summary>Applications received in the period from one source (the recruitment <c>CandidateSource</c>).</summary>
public sealed record SourceCount(string Source, int Applications);
