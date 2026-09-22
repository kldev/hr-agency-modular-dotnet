using HrAgencySystem.Workers.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Workers;

public static class WorkersModule
{
    extension(IServiceCollection services)
    {
        public void AddWorkersModule()
        {
            services.AddWorkersServices();
        }
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
