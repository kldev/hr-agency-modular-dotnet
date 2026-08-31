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
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace HrAgencySystem.Identity.Application.Handlers;

public class CreatePlatformOwnerHandler
{
    public const string EmailAlreadyUsed = "Email already used";
    
    public static async Task<PlatformOwnerCreated> Handle(
        CreatePlatformOwner command,
        IDocumentSession session,
        IClock clock,
        IPasswordHasher hasher,
        IOwnerEmailReservationRepository repository,
        CancellationToken ct)
    {
        var (email, error) = Email.TryCreate(command.Email);
        if (error != null) throw new ValidationException(error);
        
        PasswordPolicyValidator.Validate(command.Password);

        if (await repository.ExistAsync(email!, ct))
        {
            throw new BusinessRuleException(EmailAlreadyUsed);
        }

        await repository.ReserveAsync(email!);
        
        var passwordHash = hasher.Hash(command.Password);
        
        var ownerId = PlatformOwnerId.New();
        
        var @event = new PlatformOwnerCreated(
            ownerId.Value,
            email!.Value,
            PlatformRole.Owner,
            passwordHash,
            clock.UtcNow);
        
        session.Events.StartStream<PlatformOwner>(ownerId.Value, @event);
        
        return @event;
    }
}