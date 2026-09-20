using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Contacts.Assign;

public static class AssignProjectContactHandler
{
    [AggregateHandler]
    public static async Task<(ProjectContactAssigned, Wolverine.Marten.Events)> Handle(
        AssignProjectContact command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var person = ContactDataFactory.Create(new ContactData(command.Person));
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        // Assigning replaces: one person per role, so a role never quietly holds two people and
        // nobody has to guess which of them to call.
        var previous = aggregate.ContactInRole(command.Role)?.Person;

        var @event = new ProjectContactAssigned(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            command.Role,
            person,
            command.CompanyContactId,
            previous,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    private sealed record ContactData(ContactPerson Contact) : IContactData;
}
