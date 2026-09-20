using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Application.Suggestion;
using HrAgencySystem.Teams.Infrastructure.Persistence;
using HrAgencySystem.Teams.Infrastructure.Query;
using HrAgencySystem.Teams.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Teams.Infrastructure.Configuration;

internal static class TeamsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddTeamsServices()
        {
            services.AddScoped<ITeamsQueryRepository, TeamsQueryRepository>();
            services.AddScoped<ITeamSuggestionRepository, TeamSuggestionRepository>();
            services.AddScoped<ITeamsService, TeamsService>();
            services.AddScoped<ITeamSnapshotRepository, TeamSnapshotRepository>();
            services.AddScoped<
                ITeamMembershipReservationRepository,
                TeamMembershipReservationRepository
            >();
        }
    }
}
