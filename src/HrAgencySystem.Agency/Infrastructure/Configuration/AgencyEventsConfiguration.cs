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

            options.Events.AddEventType<AgencyEmploymentStarted>();
            options.Events.AddEventType<AgencyEmploymentTermsChanged>();
            options.Events.AddEventType<AgencyEmploymentEnded>();

            options.Events.AddEventType<TimeSheetStarted>();
            options.Events.AddEventType<WorkDaySaved>();
            options.Events.AddEventType<WorkDayRemoved>();
            options.Events.AddEventType<TimeSheetSubmitted>();
            options.Events.AddEventType<TimeSheetApproved>();
            options.Events.AddEventType<TimeSheetReturnedForCorrection>();
            options.Events.AddEventType<TimeSheetSettled>();
            options.Events.AddEventType<TimeSheetCommented>();
        }
    }
}
