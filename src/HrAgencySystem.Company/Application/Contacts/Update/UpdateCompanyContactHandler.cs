using HrAgencySystem.Company.Application.Contacts.Create;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Company.Application.Contacts.Update;

public static class UpdateCompanyContactHandler
{
    public static async Task<CompanyContact> Handle(
        UpdateCompanyContact command,
        ICompanyContactRepository repository,
        IClock clock, CancellationToken ct)
    {
        var data = ContactDataFactory.Create(command);
        var contact = await repository.GetById(command.ContactId, command.OrganizationId, ct);

        if (contact == null)
            throw new NotFoundException("Company contact", command.ContactId);

        var updateContact = contact with
        {
            FirstName = data.FirstName.Value,
            LastName = data.LastName.Value,
            Email = data.LastName.Value,
            JobTitle = data.JobTitle.Value,
            Phone = data.Phone.Value,
            ModifiedAt = clock.UtcNow
        };
        
        await repository.Update(updateContact);

        return updateContact;
    }
}