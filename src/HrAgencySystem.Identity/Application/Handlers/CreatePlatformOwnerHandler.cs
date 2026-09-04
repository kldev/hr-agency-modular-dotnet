using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;


namespace HrAgencySystem.Identity.Application.Handlers;

public class CreatePlatformOwnerHandler
{
    private const string EmailAlreadyUsed = "Email already used";
    
    public static async Task<PlatformOwnerCreated> Handle(
        CreatePlatformOwner command,
        IDocumentSession session,
        IClock clock,
        IPasswordHasher hasher,
        IOwnerEmailReservationRepository repository,
        CancellationToken ct)
    {
        var email = GetEmail(command);

        PasswordPolicyValidator.Validate(command.Password);

        await ValidateEmailReservation(repository, ct, email);

        var ownerId = PlatformOwnerId.New();
        
        var passwordHash = hasher.Hash(command.Password);
        await repository.ReserveAsync(email!, passwordHash, ownerId);
        
        var @event = new PlatformOwnerCreated(
            ownerId.Value,
            email.Value,
            PlatformRole.Owner,
            passwordHash,
            clock.UtcNow);
        
        session.Events.StartStream<PlatformOwner>(ownerId.Value, @event);
        
        return @event;
    }

    private static Email GetEmail(CreatePlatformOwner command)
    {
        var (email, error) = Email.TryCreate(command.Email);
        return error != null ? throw new ValidationException(error) : email!;
    }

    private static async Task ValidateEmailReservation(IOwnerEmailReservationRepository repository, CancellationToken ct,
        Email? email)
    {
        if (await repository.ExistAsync(email!, ct))
        {
            throw new BusinessRuleException(EmailAlreadyUsed);
        }
    }
}