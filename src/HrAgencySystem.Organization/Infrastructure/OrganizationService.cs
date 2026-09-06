using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Services;
using Marten;

namespace HrAgencySystem.Organization.Infrastructure;

public class OrganizationService(IQuerySession session) : IOrganizationService
{
    public async Task<IReadOnlyList<OrganizationInfo>> GetActiveOrganizationsAsync(CancellationToken ct)
    {
        return await session.Query<OrganizationCreated>()
            .Select(s => new OrganizationInfo(s.OrganizationId, s.Slug, s.Name))
            .ToListAsync(ct);
    }

    public async Task<OrganizationInfo?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var normalizeSlug = OrganizationSlug.Create(slug);
        return await session.Query<OrganizationCreated>()
            .Where(z => z.Slug == normalizeSlug.Value)
            .Select(s => new OrganizationInfo(s.OrganizationId, s.Slug, s.Name))
            .FirstOrDefaultAsync(ct);
    }
}