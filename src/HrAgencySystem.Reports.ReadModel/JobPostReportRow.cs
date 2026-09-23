namespace HrAgencySystem.Reports.ReadModel;

/// <summary>
/// One job post. <see cref="FirstPublishedAt"/> is when it first went out, so a post published,
/// closed and published again still counts once in the month it first reached candidates.
/// </summary>
public sealed class JobPostReportRow
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid CompanyId { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? FirstPublishedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
