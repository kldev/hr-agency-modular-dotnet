using HrAgencySystem.Organization.Events;

namespace HrAgencySystem.Organization.Projections;

public sealed record OrganizationProjection(Guid Id, 
    string Name, 
    string Slug, 
    DateTimeOffset CreatedAt, 
    DateTimeOffset? ModifiedAt)
{
    public static OrganizationProjection Create(
        OrganizationCreated @event)
    {
        return new OrganizationProjection(@event.OrganizationId, 
            @event.Name, 
            @event.Slug, 
            @event.CreatedAt,
            null);

    }

    public OrganizationProjection Apply(OrganizationSlugUpdated @event)
    {
        return this with
        {
            Slug = @event.Slug,
            ModifiedAt = @event.ModifiedAt
        };
    }
}
