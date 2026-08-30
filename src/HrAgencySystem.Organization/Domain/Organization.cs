using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Organization.Domain;

public sealed class Organization
{
    private Organization()
    {
    }

    public static Organization Empty()
    {
        return new Organization();
    }
    
    public OrganizationId Id { get; private set; }

    public OrganizationName Name { get; private set; } = null!;

    public OrganizationSlug Slug { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public void Apply(OrganizationCreated @event)
    {
        Id = OrganizationId.From(@event.OrganizationId);
        Name = OrganizationName.Create(@event.Name);
        Slug = OrganizationSlug.Create(@event.Slug);
        CreatedAt = @event.CreatedAt;
    }
    
    public void Apply(OrganizationSlugUpdated @event)
    {
        Slug = OrganizationSlug.Create(@event.Slug);
    }
}