using HrAgencySystem.Teams.Contracts.IntegrationCommands;
using HrAgencySystem.Teams.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

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

    /// <summary>
    /// A local queue is an in-memory queue by default, so a restart between the commit and the
    /// delivery would drop the request for good — the user would keep a team on their read model
    /// that the roster never received them into, with nothing left to retry and no dead letter.
    /// </summary>
    public static void ConfigureWolverine(WolverineOptions options)
    {
        options.LocalQueueFor<AssignUserToTeam>().UseDurableInbox();
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
