using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;

namespace HrAgencySystem.Identity.Projections;

public sealed record OwnerProjection(Guid Id, string Email, PlatformRole Role)
{
    public static OwnerProjection Create(PlatformOwnerCreated @event)
    {
        return new OwnerProjection(@event.PlatformOwnerId, @event.Email, @event.Role);
    }
}