using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Identity.Application.Users.Create;

public static class CreateUserHandler
{
    public const string UserWithEmailMessage = "A user with this email already exists in the organization.";
    
    public static async Task<UserCreated> Handle(
        CreateUser command,
        IDocumentSession session,
        IPasswordHasher hasher,
        IUserEmailReservationRepository repository,
        IIdentityService service,
        IClock clock,
        CancellationToken ct)
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var (
            email,
            firstName,
            lastName
            ) = CreateValueObjects(command);

        PasswordPolicyValidator.Validate(command.Password);

        var user = await service.GetUserAsync(command.CreatedBy, ct);
        var organizationId = OrganizationId.From(command.OrganizationId);
        
        await ValidateEmailReservation(repository, ct,organizationId , email);
        
        var userId = UserId.New();
        
        var passwordHash = hasher.Hash(command.Password);

        var organizationInfo = await service.GetOrganization(organizationId, ct);

        await repository.ReserveAsync(organizationId, email, userId, passwordHash);
        
        var @event = new UserCreated(
            userId.Value,
            organizationId.Value,
            email.Value,
            firstName.Value,
            lastName.Value,
            command.Role,
            passwordHash,
            organizationInfo,
            user!,
            clock.UtcNow);

        session.Events.StartStream<User>(
            userId.Value,
            @event);

        return @event;
    }

    private static async Task ValidateEmailReservation(IUserEmailReservationRepository repository, CancellationToken ct,
        OrganizationId organizationId, Email email)
    {
        if (await repository.ExistAsync(organizationId, email, ct))
            throw new BusinessRuleException(UserWithEmailMessage);
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