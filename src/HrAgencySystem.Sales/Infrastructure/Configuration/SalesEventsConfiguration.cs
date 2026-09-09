using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.Sales.Events.Opportunity;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Configuration;

internal static class SalesEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureSalesEvents()
        {
            ConfigureActivityEvents(options);
            ConfigureOpportunityEvents(options);
        }
    }

    private static void ConfigureActivityEvents(StoreOptions options)
    {
        options.Events.AddEventType<ActivityCreated>();
    }
    
    private static void ConfigureOpportunityEvents(StoreOptions options)
    {
        options.Events.AddEventType<OpportunityCreated>();
    }
}