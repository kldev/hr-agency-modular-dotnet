using HrAgencySystem.Projects.Events;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Configuration;

internal static class ProjectsEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<ProjectCreated>();
            options.Events.AddEventType<ProjectUpdated>();
            options.Events.AddEventType<ProjectStatusChanged>();
            options.Events.AddEventType<ProjectTeamAssigned>();
            options.Events.AddEventType<ProjectLegalEntityChanged>();
            options.Events.AddEventType<ProjectContactAssigned>();
            options.Events.AddEventType<ProjectContactRemoved>();
            options.Events.AddEventType<ProjectEmailRecipientsChanged>();
            options.Events.AddEventType<ProjectContractRecorded>();
            options.Events.AddEventType<ProjectContractStatusChanged>();
            options.Events.AddEventType<ProjectDocumentAttached>();
            options.Events.AddEventType<ProjectDocumentMetadataChanged>();
            options.Events.AddEventType<ProjectDocumentRemoved>();
            options.Events.AddEventType<ProjectPositionOpened>();
            options.Events.AddEventType<ProjectPositionUpdated>();
            options.Events.AddEventType<ProjectPositionArchived>();
            options.Events.AddEventType<ProjectPositionRestored>();
            options.Events.AddEventType<ComplianceItemRecorded>();
        }
    }
}
