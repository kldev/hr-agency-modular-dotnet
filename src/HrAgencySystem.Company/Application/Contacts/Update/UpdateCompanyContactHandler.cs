using HrAgencySystem.Company.Application.Contacts.Create;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine;

namespace HrAgencySystem.Company.Application.Contacts.Update;

public static class UpdateCompanyContactHandler
{
    public static async Task<(CompanyContact, Wolverine.Marten.Events)> Handle(
        UpdateCompanyContact command,
        ICompanyContactRepository repository,
        IMessageBus bus,
        IClock clock, CancellationToken ct)
    {
        var data = ContactDataFactory.Create(command);
        var contact = await repository.GetById(command.ContactId, command.OrganizationId, ct);

        if (contact == null)
            throw new NotFoundException("Company contact", command.ContactId);

        var updateContact = contact with
        {
            Contact = data,
            ModifiedAt = clock.UtcNow
        };

        await repository.Update(updateContact);

        if (!command.UpdatePrimary) return (updateContact, []);
        
        var @event = new CompanyPrimaryContactUpdated(contact.CompanyId, contact.OrganizationId, data, contact.Id,
            clock.UtcNow);

        return (updateContact, [@event]);

    }
}