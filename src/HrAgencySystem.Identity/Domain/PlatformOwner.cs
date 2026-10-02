using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.ValueObjects;
using JetBrains.Annotations;

namespace HrAgencySystem.Identity.Domain;

public sealed class PlatformOwner
{
    [UsedImplicitly] // Marten rebuilds the aggregate through it.
    private PlatformOwner() { }

    public PlatformOwnerId Id { get; private set; }

    public Email Email { get; private set; } = null!;

    public PlatformRole Role { get; private set; }

    public string PasswordHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public void Apply(PlatformOwnerCreated @event)
    {
        Id = PlatformOwnerId.From(@event.PlatformOwnerId);

        Email = Email.Create(@event.Email);

        Role = @event.Role;

        PasswordHash = @event.PasswordHash;

        CreatedAt = @event.CreatedAt;
    }
}
