using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Infrastructure.Persistence;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Sales.Infrastructure.Configuration;

internal static class SalesServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddSalesServices()
        {
            services.AddScoped<ISalesPipelineQueryRepository, SalesPipelineQueryRepository>();
            services.AddScoped<
                IOpportunitySnapshotRepository,
                SalesOpportunitySnapshotRepository
            >();
            services.AddScoped<ISalesActivityQueryRepository, SalesActivityQueryRepository>();
            services.AddScoped<ISalesService, SalesService>();
            services.AddScoped<IOpportunityQueryRepository, OpportunityQueryRepository>();
            services.AddScoped<IQueryFollowUpAction, QueryFollowUpAction>();
        }
    }
}
