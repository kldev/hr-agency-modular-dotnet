namespace HrAgencySystem.Reports.ReadModel;

/// <summary>
/// One tenant, as the platform report lists it. Written by the Organization module.
/// </summary>
public sealed class OrganizationReportRow
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = "";

    public string Slug { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
