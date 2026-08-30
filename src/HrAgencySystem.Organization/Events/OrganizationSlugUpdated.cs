namespace HrAgencySystem.Organization.Events;

public sealed record OrganizationSlugUpdated(string Slug, Guid OrganizationId);