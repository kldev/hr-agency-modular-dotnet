using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Query;

public sealed class CompanyContactQueryRepository(IQuerySession session) : ICompanyContactQueryRepository
{
    public async Task<IReadOnlyList<CompanyContact>> GetAllAsync(Guid organizationId, Guid companyId, CancellationToken ct)
    {
        return await session.Query<CompanyContact>()
            .WithOrganizationId(OrganizationId.From(organizationId))
            .WithCompanyId(CompanyId.From(companyId))
            .OrderBy(z=>z.Contact.FirstName)
            .ThenByDescending(z=>z.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<CompanyContact?> GetAsync(Guid organizationId, Guid contactId, CancellationToken ct)
    {
        return await session.Query<CompanyContact>()
            .WithOrganizationId(OrganizationId.From(organizationId))
            .WithContactId(contactId)
            .FirstOrDefaultAsync(ct);
    }
}