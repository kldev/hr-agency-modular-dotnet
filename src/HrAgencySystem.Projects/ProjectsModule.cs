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

    // No Minimal variant: the public job board has no idea projects exist, and no reason to.
    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
