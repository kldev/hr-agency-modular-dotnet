using HrAgencySystem.Forms.Events;
using Marten;

namespace HrAgencySystem.Forms.Infrastructure.Configuration;

internal static class FormsEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<FormDefinitionCreated>();
            options.Events.AddEventType<FormDetailsUpdated>();
            options.Events.AddEventType<FormDraftSaved>();
            options.Events.AddEventType<FormPublished>();
            options.Events.AddEventType<FormArchived>();

            options.Events.AddEventType<SystemFieldDefined>();
            options.Events.AddEventType<SystemFieldUpdated>();
            options.Events.AddEventType<SystemFieldArchived>();

            options.Events.AddEventType<FormResponseStarted>();
            options.Events.AddEventType<FormResponseDraftSaved>();
            options.Events.AddEventType<FormResponseSubmitted>();
            options.Events.AddEventType<FormResponseCorrected>();
        }
    }
}
