using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Identity.Application.Users.Update;

public static class UpdateUserHandler
{
    [AggregateHandler]
    public static async Task<(UserUpdated, Wolverine.Marten.Events)> Handle(
        UpdateUser command,
        User aggregate,
        IIdentityService service,
        IUserEmailReservationRepository repository,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId.Value);

        var contact = ContactDataFactory.CreateValueObjects(command);

        if (contact.Email.Value != aggregate.Email.Value)
        {
            var exists = await repository.ExistAsync(aggregate.OrganizationId, contact.Email, ct);
            if (exists)
                throw new BusinessRuleException(CreateUserHandler.UserWithEmailMessage);
        }

        var user =
            command.ModifiedBy == Guid.Empty
                ? UserSnapshot.System
                : await service.GetUserAsync(command.ModifiedBy, ct);

        await repository.ChangeEmailAsync(
            aggregate.OrganizationId,
            aggregate.Id,
            contact.Email,
            ct
        );

        var @event = new UserUpdated(
            command.UserId,
            aggregate.OrganizationId.Value,
            user,
            contact.ToContact(),
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
