using HrAgencySystem.Identity.Events;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Configuration;

internal static class IdentityEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<UserCreated>();
            options.Events.AddEventType<PlatformOwnerCreated>();
            options.Events.AddEventType<RoleChanged>();
            options.Events.AddEventType<UserUpdated>();
        }
    }
}
