using HrAgencySystem.Projects.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Projects;

public static class ProjectsModule
{
    extension(IServiceCollection services)
    {
        public void AddProjectsModule()
        {
            services.AddProjectsServices();
        }
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
