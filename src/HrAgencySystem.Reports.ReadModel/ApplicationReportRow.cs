namespace HrAgencySystem.Reports.ReadModel;

/// <summary>
/// One job application and the stages it has reached.
/// <para>
/// Every <c>*At</c> stage column holds the <b>first</b> time the application got there and is never
/// overwritten, so the funnel can ask "how many applications from March reached an offer" -
/// while <see cref="Status"/> answers "where is it now". An application that went back to
/// screening keeps its interview date.
/// </para>
/// </summary>
public sealed class ApplicationReportRow
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid JobPostId { get; set; }

    /// <summary>The recruitment <c>CandidateSource</c> by name - this project knows no enums.</summary>
    public string Source { get; set; } = "";

    /// <summary>The recruitment <c>JobApplicationStatus</c> by name.</summary>
    public string Status { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ScreeningAt { get; set; }

    public DateTimeOffset? InterviewAt { get; set; }

    public DateTimeOffset? AssessmentAt { get; set; }

    public DateTimeOffset? OfferAt { get; set; }

    public DateTimeOffset? HiredAt { get; set; }

    public DateTimeOffset? RejectedAt { get; set; }

    public DateTimeOffset? WithdrawnAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
