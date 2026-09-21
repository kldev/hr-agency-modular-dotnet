using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Application.Suggestion;
using HrAgencySystem.Projects.Infrastructure.Query;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Projects.Infrastructure.Configuration;

internal static class ProjectsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddProjectsServices()
        {
            services.AddScoped<IProjectsQueryRepository, ProjectsQueryRepository>();
            services.AddScoped<IProjectSuggestionRepository, ProjectSuggestionRepository>();
            services.AddScoped<IPositionsQueryRepository, PositionsQueryRepository>();
            services.AddScoped<IPositionSuggestionRepository, PositionSuggestionRepository>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectSnapshotRepository, ProjectSnapshotRepository>();
            services.AddScoped<IPositionSnapshotRepository, PositionSnapshotRepository>();
        }
    }
}
