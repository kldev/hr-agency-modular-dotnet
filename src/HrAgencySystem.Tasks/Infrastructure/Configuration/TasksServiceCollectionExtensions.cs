using HrAgencySystem.Tasks.Application.Port;
using HrAgencySystem.Tasks.Infrastructure.Query;
using HrAgencySystem.Tasks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Tasks.Infrastructure.Configuration;

internal static class TasksServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddTasksServices()
        {
            services.AddScoped<ITasksService, TasksService>();
            services.AddScoped<ITaskItemsQueryRepository, TaskItemsQueryRepository>();
        }
    }
}
