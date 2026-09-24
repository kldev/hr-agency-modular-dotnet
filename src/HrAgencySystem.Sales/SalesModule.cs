using HrAgencySystem.Sales.Infrastructure.Configuration;
using HrAgencySystem.Tasks.Contracts.IntegrationEvents;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace HrAgencySystem.Sales;

public static class SalesModule
{
    public static void AddSalesModule(this IServiceCollection services)
    {
        services.AddSalesServices();
    }

    /// <summary>
    /// A local queue is in memory by default: a restart between the task being ticked off and this
    /// module hearing about it would lose the history entry with nothing left to retry.
    /// </summary>
    public static void ConfigureWolverine(WolverineOptions options)
    {
        options.LocalQueueFor<OpportunityTaskCompleted>().UseDurableInbox();
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureSalesDocuments();
        options.ConfigureSalesEvents();
        options.ConfigureSalesProjections();
    }
}
