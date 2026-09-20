using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine.Marten;

namespace HrAgencySystem.Identity.Application.Users.UpdateOwnProfile;

public static class UpdateOwnProfileHandler
{
    [AggregateHandler]
    public static async Task<(UserUpdated, Wolverine.Marten.Events)> Handle(
        UpdateOwnProfile command,
        User aggregate,
        IIdentityService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId.Value);

        // The address comes off the aggregate, never off the command or the token. A claim can be
        // older than the record - if an administrator changed the e-mail in the meantime, taking it
        // from anywhere else would quietly put the old one back on the next save.
        var contact = ContactDataFactory.CreateValueObjects(
            new ProfileContact(
                new ContactPerson(
                    aggregate.Email.Value,
                    command.FirstName,
                    command.LastName,
                    command.JobTitle,
                    command.Phone
                )
            )
        );

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        // The same event an administrator's edit raises: the fact is identical, and ModifiedBy
        // already says which of the two happened. No e-mail reservation is touched, because by
        // construction the address cannot have changed.
        var @event = new UserUpdated(
            command.UserId,
            aggregate.OrganizationId.Value,
            user,
            contact.ToContact(),
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    private sealed record ProfileContact(ContactPerson Contact) : IContactData;
}
