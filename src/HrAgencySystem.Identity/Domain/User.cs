using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Identity.Domain;

public sealed class User : IOrganizationDomain
{
    private User() { }

    public UserId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public Email Email { get; private set; } = null!;

    public FirstName FirstName { get; private set; } = null!;

    public LastName LastName { get; private set; } = null!;

    public PersonJobTitle JobTitle { get; private set; } = null!;

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

        Email = Email.Create(@event.Contact.Email);
        FirstName = FirstName.Create(@event.Contact.FirstName);
        LastName = LastName.Create(@event.Contact.LastName);
        JobTitle = PersonJobTitle.Create(@event.Contact.JobTitle);

        Role = @event.Role;

        PasswordHash = @event.PasswordHash;

        CreatedAt = @event.CreatedAt;
    }

    public void Apply(UserUpdated @event)
    {
        Email = Email.Create(@event.Contact.Email);
        FirstName = FirstName.Create(@event.Contact.FirstName);
        LastName = LastName.Create(@event.Contact.LastName);
        JobTitle = PersonJobTitle.Create(@event.Contact.JobTitle);
    }

    public void Apply(RoleChanged @event)
    {
        Role = @event.Role;
    }
}
