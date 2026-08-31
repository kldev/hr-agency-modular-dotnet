using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Identity.Domain;

public sealed class User
{
    private User()
    {
    }

    public UserId Id { get; private set; } = null!;

    public OrganizationId OrganizationId { get; private set; }

    public Email Email { get; private set; } = null!;

    public FirstName FirstName { get; private set; } = null!;

    public LastName LastName { get; private set; } = null!;

    public OrganizationRole Role { get; private set; }

    public string PasswordHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public static User Empty()
    {
        return new User();
    }

    public void Apply(UserCreated @event)
    {
        Id = UserId.From(@event.UserId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);

        Email = Email.Create(@event.Email);
        FirstName = FirstName.Create(@event.FirstName);
        LastName = LastName.Create(@event.LastName);

        Role = @event.Role;

        PasswordHash = @event.PasswordHash;

        CreatedAt = @event.CreatedAt;
    }
}