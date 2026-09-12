using HrAgencySystem.Organization.Events;

namespace HrAgencySystem.Organization.Projections;

public sealed record OrganizationProjection(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id, 
    string Name, 
    string Slug, 
    DateTimeOffset CreatedAt, 
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset? ModifiedAt,
    IReadOnlyList<string> EmailDomains)
{
    public static OrganizationProjection Create(
        OrganizationCreated @event)
    {
        return new OrganizationProjection(@event.OrganizationId, 
            @event.Name, 
            @event.Slug, 
            @event.CreatedAt,
            null,
            @event.EmailDomains);

    }

    public OrganizationProjection Apply(OrganizationSlugUpdated @event)
    {
        return this with
        {
            Slug = @event.Slug,
            ModifiedAt = @event.ModifiedAt
        };
    }
    
    public OrganizationProjection Apply(OrganizationUpdated @event)
    {
        return this with
        {
            Slug = @event.Slug,
            Name = @event.Name,
            EmailDomains = @event.EmailDomains,
            ModifiedAt = @event.ModifiedAt
        };
    }
}
