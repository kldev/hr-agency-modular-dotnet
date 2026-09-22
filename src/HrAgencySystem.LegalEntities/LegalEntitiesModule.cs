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

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
