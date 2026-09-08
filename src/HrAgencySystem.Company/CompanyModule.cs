using HrAgencySystem.Company.Infrastructure.Configuration;
using HrAgencySystem.Company.Infrastructure.Query;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Company;

public static class CompanyModule
{
    extension(IServiceCollection services)
    {
        public void AddCompanyModule()
        {
            services.AddCompanyServices();
        }
    }

    public static void AddCompanyMinimalModule(this IServiceCollection services)
    {
        services.AddScoped<ICompanySnapshotRepository, CompanySnapshotRepository>();
    }

    public static void ConfigureMartenMinimal(StoreOptions options)
    {
        options.ConfigureCompanyProjections();
    }

    public static void ConfigureMarten(
        StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
    
}