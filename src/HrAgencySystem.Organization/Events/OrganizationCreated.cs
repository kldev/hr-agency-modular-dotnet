namespace HrAgencySystem.Organization.Events;

public sealed record OrganizationCreated(
    Guid OrganizationId,
    string Name,
    string Slug,
    DateTimeOffset CreatedAt);