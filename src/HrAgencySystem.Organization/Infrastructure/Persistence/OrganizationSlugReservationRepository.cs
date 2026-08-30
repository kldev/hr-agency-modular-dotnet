using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Organization.Infrastructure.Persistence;

// ReSharper disable once ClassNeverInstantiated.Global
public class OrganizationSlugReservationRepository(IDocumentSession session) : IOrganizationSlugReservationRepository
{
    public async Task<bool> Exists(OrganizationSlug slug, CancellationToken ct)
    {
        return await session.Query<OrganizationSlugReservation>().Where(z=>z.Slug == slug.Value).AnyAsync(ct);
    }

    public Task Reserve(OrganizationId organizationId, OrganizationSlug slug)
    {
        var item = new OrganizationSlugReservation
        {
            Slug = slug.Value, 
            OrganizationId = organizationId.Value
        };
        
        session.Insert(item);

        return Task.CompletedTask;
    }
}