using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Wolverine;

namespace HrAgencySystem.Identity.Application.Handlers;

public static class CreateUserHandler
{
    public const string OrganizationCheckMessage = "Non existing organization.";
    public const string UserWithEmailMessage = "A user with this email already exists in the organization.";
    
    public static async Task<UserCreated> Handle(
        CreateUser command,
        IDocumentSession session,
        IClock clock,
        IOrganizationChecker checker,
        IPasswordHasher hasher,
        IUserEmailReservationRepository repository,
        CancellationToken ct)
    {
        var organizationId =
            OrganizationId.From(command.OrganizationId);

        if (!await checker.Exists(command.OrganizationId, ct))
            throw new BusinessRuleException(OrganizationCheckMessage);

        var (
            email,
            firstName,
            lastName
            ) = CreateValueObjects(command);

        PasswordPolicyValidator.Validate(command.Password);

        if (await repository.ExistAsync(organizationId, email, ct))
            throw new BusinessRuleException(UserWithEmailMessage);
        
        var userId = UserId.New();
        
        var passwordHash = hasher.Hash(command.Password);

        await repository.ReserveAsync(organizationId, email, userId, passwordHash);
        
        var @event = new UserCreated(
            userId.Value,
            organizationId.Value,
            email.Value,
            firstName.Value,
            lastName.Value,
            command.Role,
            passwordHash,
            clock.UtcNow);

        session.Events.StartStream<User>(
            userId.Value,
            @event);

        return @event;
    }

    private static UserData CreateValueObjects(
        CreateUser command)
    {
        var errors = new List<string>();

        var (email, emailError) =
            Email.TryCreate(command.Email);

        if (emailError is not null)
            errors.Add(emailError);

        var (firstName, firstNameError) =
            FirstName.TryCreate(command.FirstName);

        if (firstNameError is not null)
            errors.Add(firstNameError);

        var (lastName, lastNameError) =
            LastName.TryCreate(command.LastName);

        if (lastNameError is not null)
            errors.Add(lastNameError);
        
        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new UserData(
            email!,
            firstName!,
            lastName!);
    }

    private sealed record UserData(
        Email Email,
        FirstName FirstName,
        LastName LastName);
}