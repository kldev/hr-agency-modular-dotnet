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

    // No Minimal variant: the public job board shows offers to candidates, and a candidate has no
    // business seeing who the agency already has on its books.
    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
