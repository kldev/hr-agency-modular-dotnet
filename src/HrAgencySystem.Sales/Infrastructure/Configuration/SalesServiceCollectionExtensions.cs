using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Sales.Infrastructure.Configuration;

internal static class SalesServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddSalesServices()
        {
            services.AddScoped<ISalesPipelineQueryRepository, SalesPipelineQueryRepository>();
            services.AddScoped<ISalesOpportunitySnapshotRepository, SalesOpportunitySnapshotRepository>();
        }
    }
}