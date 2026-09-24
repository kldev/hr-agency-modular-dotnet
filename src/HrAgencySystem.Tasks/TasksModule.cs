using HrAgencySystem.Tasks.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Tasks;

/// <summary>
/// Work a person at the agency has to do by a given time, for a client company - the salesperson's
/// to-do list (plan 029). A task always names a company and may name one of its deals; both are
/// read through SharedKernel ports, never referenced.
/// <para>
/// Not the sales follow-up, which stays in <c>Sales</c>: a follow-up is the next step noted on a
/// deal, a task has an owner, a priority and is done or not.
/// </para>
/// </summary>
public static class TasksModule
{
    extension(IServiceCollection services)
    {
        public void AddTasksModule()
        {
            services.AddTasksServices();
        }
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
