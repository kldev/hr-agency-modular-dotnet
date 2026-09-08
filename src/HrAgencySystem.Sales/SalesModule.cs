using HrAgencySystem.Sales.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Sales;

public static class SalesModule
{
    public static void AddSalesModule(
        this IServiceCollection services)
    {
        services.AddSalesServices();
    }

    public static void ConfigureMarten(
        StoreOptions options)
    {
        options.ConfigureSalesEvents();
        options.ConfigureSalesProjections();
    }
}