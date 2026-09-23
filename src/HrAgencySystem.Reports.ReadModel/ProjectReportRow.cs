namespace HrAgencySystem.Reports.ReadModel;

/// <summary>
/// One project. <see cref="WentLiveAt"/> is the first time it became active - a suspended project
/// that comes back is still one project that went live once.
/// </summary>
public sealed class ProjectReportRow
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid CompanyId { get; set; }

    /// <summary>The compliance <c>EngagementType</c> by name.</summary>
    public string EngagementType { get; set; } = "";

    public string CountryCode { get; set; } = "";

    /// <summary>The projects <c>ProjectStatus</c> by name.</summary>
    public string Status { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? WentLiveAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
