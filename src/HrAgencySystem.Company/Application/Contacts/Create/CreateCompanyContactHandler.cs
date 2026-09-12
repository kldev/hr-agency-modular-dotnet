using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Company.Application.Contacts.Create;

public static class CreateCompanyContactHandler
{
    
    public const string ContactWithEmailMessage = "Contact with this email already exists in the company.";
    
    public static async Task<CompanyContact> Handle(CreateCompanyContact command,
        ICompanySnapshotRepository snapshotRepository,
        ICompanyContactRepository repository,
        IClock clock, CancellationToken ct)
    {
        var company = await snapshotRepository.GetCompanyAsync(command.CompanyId, ct);
        if (company is null) throw new NotFoundException("Company", command.CompanyId);

        var data = ContactDataFactory.Create(command);

        var contact = new CompanyContact(
            Guid.NewGuid(),
            command.OrganizationId,
            command.CompanyId,
            data, company!.Name, clock.UtcNow);

        await repository.Create(contact);

        return contact;
    }
}