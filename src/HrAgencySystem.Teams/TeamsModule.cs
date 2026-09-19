using HrAgencySystem.Teams.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Teams;

public static class TeamsModule
{
    extension(IServiceCollection services)
    {
        public void AddTeamsModule()
        {
            services.AddTeamsServices();
        }
    }

    // No Minimal variant: the public job board shows offers to candidates, and a candidate never
    // sees a team.
    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
