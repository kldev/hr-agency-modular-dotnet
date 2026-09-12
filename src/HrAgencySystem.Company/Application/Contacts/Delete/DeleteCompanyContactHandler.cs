using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine;

namespace HrAgencySystem.Company.Application.Contacts.Delete;


public static class UpdateCompanyContactHandler
{
    public static async Task<CompanyContactDeleted> Handle(
        DeleteCompanyContact command,
        ICompanyContactRepository repository,
        IMessageBus bus,
        IClock clock, CancellationToken ct)
    {
        var contact = await repository.GetById(command.ContactId, command.OrganizationId, ct);

        if (contact == null)
            throw new NotFoundException("Company contact", command.ContactId);
        
        await repository.Delete(contact);
        
        return new CompanyContactDeleted(contact.Id, clock.UtcNow);

    }
}