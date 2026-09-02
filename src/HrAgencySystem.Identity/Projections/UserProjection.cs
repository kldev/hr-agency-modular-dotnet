using HrAgencySystem.Identity.Application.Model;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Identity.Projections;

public sealed record UserProjection(Guid Id, Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt)
{
    public static UserProjection Create(UserCreated @event)
    {
        return new UserProjection(@event.UserId, 
            @event.OrganizationId, 
            @event.Email,
            @event.FirstName, 
            @event.LastName,
            @event.Role,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt);
    }

    public UserSuggestion ToSuggestion() =>
        new (Id, $"{FirstName} {LastName}", Email);

}