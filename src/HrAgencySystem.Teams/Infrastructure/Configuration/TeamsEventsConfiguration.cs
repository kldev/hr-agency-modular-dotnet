using HrAgencySystem.Teams.Events;
using Marten;

namespace HrAgencySystem.Teams.Infrastructure.Configuration;

internal static class TeamsEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<TeamCreated>();
            options.Events.AddEventType<TeamRenamed>();
            options.Events.AddEventType<TeamMemberAdded>();
            options.Events.AddEventType<TeamMemberRemoved>();
            options.Events.AddEventType<TeamMemberRoleChanged>();
        }
    }
}
