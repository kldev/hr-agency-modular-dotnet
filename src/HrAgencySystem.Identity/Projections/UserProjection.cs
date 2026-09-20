using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Contracts;

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
    string JobTitle = "",
    TeamInfo? Team = null
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
            @event.Contact.JobTitle,
            @event.Team
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
            Phone = @event.Contact.Phone,
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

    /// <summary>
    /// Membership is not an edit of the user record, so it deliberately leaves ModifiedBy/ModifiedAt
    /// alone — those answer "who last changed this person's details", and nobody did.
    /// </summary>
    public UserProjection Apply(UserTeamChanged @event)
    {
        return this with { Team = @event.Team };
    }

    public UserProjection Apply(PasswordChanged @event)
    {
        return this with { ModifiedBy = @event.ModifiedBy, ModifiedAt = @event.ModifiedAt };
    }

    public string FullName => $"{FirstName} {LastName}";
}
