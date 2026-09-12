namespace HrAgencySystem.Organization.Events;

public sealed record OrganizationUpdated(
    Guid OrganizationId,
    string Name,
    string Slug,
    IReadOnlyList<string> EmailDomains,
    DateTimeOffset ModifiedAt);