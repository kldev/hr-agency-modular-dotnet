using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace HrAgencySystem.Organization.Projections;

public  partial class OrganizationProjection : SingleStreamProjection<Domain.Organization,Guid>
{
    // ReSharper disable once MemberCanBePrivate.Global
    public OrganizationProjection()
    {
        // Make sure this is turned on!
        Options.CacheLimitPerTenant = 1000;
    }
    
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }
    
    public OrganizationProjection Create(IEvent<OrganizationCreated> @event)
    {
        return new OrganizationProjection()
        {
            OrganizationId = @event.Data.OrganizationId,
            Name = @event.Data.Name,
            Slug = @event.Data.Slug,
            CreatedAt = @event.Data.CreatedAt
        };
    }
    
    public void Apply(OrganizationSlugUpdated @event, OrganizationProjection org)
    {
        org.Slug = @event.Slug;
        org.ModifiedAt = @event.ModifiedAt;
    }
}

