using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;

namespace HrAgencySystem.Identity.Projections;

public sealed record UserProjection(Guid Id, Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role)
{
    public static UserProjection Create(UserCreated @event)
    {
        return new UserProjection(@event.UserId, 
            @event.OrganizationId, 
            @event.Email,
            @event.FirstName, 
            @event.LastName,
            @event.Role);
    }
}