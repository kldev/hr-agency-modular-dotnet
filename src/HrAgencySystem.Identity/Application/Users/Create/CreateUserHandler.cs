using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Identity.Application.Users.Create;

public static class CreateUserHandler
{
    public const string UserWithEmailMessage =
        "A user with this email already exists in the organization.";

    public static async Task<UserCreated> Handle(
        CreateUser command,
        IDocumentSession session,
        IPasswordHasher hasher,
        IUserEmailReservationRepository repository,
        IIdentityService service,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var contact = ContactDataFactory.CreateValueObjects(command);

        PasswordPolicyValidator.Validate(command.Password);

        var user = await service.GetUserAsync(command.CreatedBy, ct);
        var organizationId = OrganizationId.From(command.OrganizationId);

        await ValidateEmailReservation(repository, ct, organizationId, contact.Email);

        var userId = UserId.New();

        var passwordHash = hasher.Hash(command.Password);

        var organizationInfo = await service.GetOrganization(organizationId, ct);

        await repository.ReserveAsync(organizationId, contact.Email, userId, passwordHash);

        var @event = new UserCreated(
            userId.Value,
            organizationId.Value,
            command.Role,
            passwordHash,
            organizationInfo,
            user!,
            contact.ToContact(),
            clock.UtcNow
        );

        session.Events.StartStream<User>(userId.Value, @event);

        return @event;
    }

    private static async Task ValidateEmailReservation(
        IUserEmailReservationRepository repository,
        CancellationToken ct,
        OrganizationId organizationId,
        Email email
    )
    {
        if (await repository.ExistAsync(organizationId, email, ct))
            throw new BusinessRuleException(UserWithEmailMessage);
    }
}
