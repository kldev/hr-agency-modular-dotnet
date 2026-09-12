using HrAgencySystem.Company.Documents;

namespace HrAgencySystem.Company.Application.Port;

public interface ICompanyContactRepository
{
    Task Create(CompanyContact contact);
    Task Update(CompanyContact contact);
    Task Delete(CompanyContact contact);
    
    Task<CompanyContact?> GetById(Guid contactId, Guid organizationId, CancellationToken ct);
    
}