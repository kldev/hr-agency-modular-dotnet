using HrAgencySystem.Workers.Events;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Configuration;

internal static class WorkersEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<WorkerRegistered>();
            options.Events.AddEventType<WorkerUpdated>();
            options.Events.AddEventType<WorkerStatusChanged>();
            options.Events.AddEventType<WorkerDocumentAttached>();
            options.Events.AddEventType<WorkerDocumentMetadataChanged>();
            options.Events.AddEventType<WorkerDocumentRemoved>();
            options.Events.AddEventType<WorkAuthorisationRecorded>();
            options.Events.AddEventType<WorkAuthorisationRemoved>();

            options.Events.AddEventType<AssignmentPlanned>();
            options.Events.AddEventType<AssignmentUpdated>();
            options.Events.AddEventType<AssignmentStatusChanged>();
            options.Events.AddEventType<AssignmentPositionRenamed>();
            options.Events.AddEventType<AssignmentDocumentAttached>();
            options.Events.AddEventType<AssignmentDocumentMetadataChanged>();
            options.Events.AddEventType<AssignmentDocumentRemoved>();
            options.Events.AddEventType<AssignmentComplianceItemRecorded>();
        }
    }
}
