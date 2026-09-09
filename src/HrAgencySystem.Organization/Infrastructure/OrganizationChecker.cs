using HrAgencySystem.Organization.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Organization.Infrastructure;

public sealed class OrganizationChecker(IQuerySession session)
    : IOrganizationChecker
{
    public async Task<bool> Exists(
        Guid organizationId,
        CancellationToken ct)
    {
        return await session.Query<OrganizationSlugReservation>()
            .Where(z => z.OrganizationId == organizationId).AnyAsync(ct);
    }

    public async Task<string?> GetSlug(Guid organizationId, CancellationToken ct)
    {
        return await session.Query<OrganizationSlugReservation>()
            .Where(z => z.OrganizationId == organizationId)
            .Select(z => z.Slug).FirstOrDefaultAsync(ct);
    }

    public async Task<OrganizationId> GetOrganizationIdBySlug(string slug, CancellationToken ct)
    {
       var organizationId =   await session.Query<OrganizationSlugReservation>()
           .Where(z => z.Slug == slug)
           .Select(z => z.OrganizationId).FirstOrDefaultAsync(ct);
       if (organizationId == Guid.Empty) throw new NotFoundException("Organization", "Slug");
       return OrganizationId.From(organizationId);
    }
}