using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Projections;

public sealed record UserProjection(
    Guid Id,
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CreatedById,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    OrganizationInfo Organization,
    string Phone = "",
    UserSnapshot? ModifiedBy = null,
    DateTimeOffset? ModifiedAt = null,
    string JobTitle = ""
) : IAudit
{
    public static UserProjection Create(UserCreated @event)
    {
        return new UserProjection(
            @event.UserId,
            @event.OrganizationId,
            @event.Contact.Email,
            @event.Contact.FirstName,
            @event.Contact.LastName,
            @event.Role,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt,
            @event.Organization,
            @event.Contact.Phone,
            null,
            null,
            @event.Contact.JobTitle
        );
    }

    public UserProjection Apply(UserUpdated @event)
    {
        return this with
        {
            Email = @event.Contact.Email,
            FirstName = @event.Contact.FirstName,
            LastName = @event.Contact.LastName,
            JobTitle = @event.Contact.JobTitle,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public UserProjection Apply(RoleChanged @event)
    {
        return this with
        {
            Role = @event.Role,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public string FullName => $"{FirstName} {LastName}";
}
