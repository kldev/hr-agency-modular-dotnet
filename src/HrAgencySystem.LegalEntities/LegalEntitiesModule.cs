using HrAgencySystem.LegalEntities.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.LegalEntities;

public static class LegalEntitiesModule
{
    extension(IServiceCollection services)
    {
        public void AddLegalEntitiesModule()
        {
            services.AddLegalEntitiesServices();
        }
    }

    // No Minimal variant: the public job board shows offers to candidates, and a candidate never
    // sees which of our companies runs the engagement behind an offer.
    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
