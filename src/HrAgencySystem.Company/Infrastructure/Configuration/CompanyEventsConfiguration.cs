using HrAgencySystem.Company.Events;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Configuration;

internal static class CompanyEventsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<CompanyCreated>();
            options.Events.AddEventType<CompanyJobPostCreated>();
            options.Events.AddEventType<CompanyJobPostActiveChanged>();
        }
    }   
}