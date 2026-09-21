using HrAgencySystem.Agency.Events;
using Marten;

namespace HrAgencySystem.Agency.Infrastructure.Configuration;

internal static class AgencyEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<OrgUnitCreated>();
            options.Events.AddEventType<OrgUnitRenamed>();
            options.Events.AddEventType<OrgUnitMoved>();
            options.Events.AddEventType<OrgUnitHeadAssigned>();
            options.Events.AddEventType<OrgUnitHeadCleared>();
            options.Events.AddEventType<OrgUnitMemberAdded>();
            options.Events.AddEventType<OrgUnitMemberRemoved>();
            options.Events.AddEventType<OrgUnitArchived>();
        }
    }
}
