using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;
using HrAgencySystem.Company.Infrastructure.Query;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Persistence;

public sealed class CompanyContactRepository(IDocumentSession session) : ICompanyContactRepository
{
    public Task Create(CompanyContact contact)
    {
        session.Insert(contact);
        return Task.CompletedTask;
    }

    public Task Update(CompanyContact contact)
    {
        session.Update(contact);
        return Task.CompletedTask;
    }
    
    public Task Delete(CompanyContact contact)
    {
        session.Delete(contact);
        return Task.CompletedTask;
    }


    public async Task<CompanyContact?> GetById(Guid contactId, Guid organizationId, CancellationToken ct)
    {
        return await session.Query<CompanyContact>()
            .WithOrganizationId(OrganizationId.From(organizationId))
            .WithContactId(contactId)
            .SingleOrDefaultAsync(ct);
    }
}