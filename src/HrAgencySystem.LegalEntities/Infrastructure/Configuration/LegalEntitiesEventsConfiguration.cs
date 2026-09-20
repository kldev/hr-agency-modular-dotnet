using HrAgencySystem.LegalEntities.Events;
using Marten;

namespace HrAgencySystem.LegalEntities.Infrastructure.Configuration;

internal static class LegalEntitiesEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<LegalEntityCreated>();
            options.Events.AddEventType<LegalEntityUpdated>();
            options.Events.AddEventType<LegalEntityClosed>();
        }
    }
}
