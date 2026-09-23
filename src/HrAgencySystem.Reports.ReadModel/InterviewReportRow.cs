namespace HrAgencySystem.Reports.ReadModel;

/// <summary>One interview. <see cref="CompletedAt"/> is set once it is marked as held.</summary>
public sealed class InterviewReportRow
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid JobApplicationId { get; set; }

    /// <summary>The recruitment <c>InterviewStatus</c> by name.</summary>
    public string Status { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
