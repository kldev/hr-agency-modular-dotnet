using HrAgencySystem.Company;
using HrAgencySystem.Company.Infrastructure;
using HrAgencySystem.Organization;
using JasperFx;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Api.Infrastructure;

public static class SetupMartenExtensions
{
    public static void SetupMartenForApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(options =>
            {
                options.Connection(configuration.GetConnectionString("Postgres")!);
                options.Events.DatabaseSchemaName = "events";

                CompanyModule.ConfigureMarten(options);
                OrganizationModule.ConfigureMarten(options);

                options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
                
            })
            .IntegrateWithWolverine();
    }

    public static void SetupWolverineForApplication(this ConfigureHostBuilder builder)
    {
        builder.UseWolverine(options =>
        {
            options.Discovery.IncludeAssembly(
                typeof(CompanyModule)
                    .Assembly);
            options.Discovery.IncludeAssembly(
                typeof(OrganizationModule)
                    .Assembly);
            options.Policies.AutoApplyTransactions();
        }).StartAsync();
    }
}