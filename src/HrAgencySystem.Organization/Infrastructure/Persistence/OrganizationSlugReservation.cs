namespace HrAgencySystem.Organization.Infrastructure.Persistence;

public class OrganizationSlugReservation
{
    public Guid Id { get; init; }

    public Guid OrganizationId { get; init; }

    public string Slug { get; init; } = null!;
}