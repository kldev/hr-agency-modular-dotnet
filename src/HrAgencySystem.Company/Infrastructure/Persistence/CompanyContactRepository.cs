using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;
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
}