using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Identity.Application.Handlers;

public static class CreateUserHandler
{
    public const string OrganizationCheckMessage = "Non existing organization.";
    
    public static async Task<UserCreated> Handle(
        CreateUser command,
        IDocumentSession session,
        IClock clock,
        IOrganizationChecker checker,
        IPasswordHasher hasher,
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

        var passwordHash = hasher.Hash(command.Password);
        
        var userId = UserId.New();

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