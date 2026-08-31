using HrAgencySystem.Company;
using HrAgencySystem.Identity;
using HrAgencySystem.Organization;
using JasperFx;
using JasperFx.Events.Daemon;
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
                var connectionString = configuration.GetConnectionString("Postgres");
                
                options.Connection(connectionString!);
                
                options.Events.DatabaseSchemaName = "events";

                CompanyModule.ConfigureMarten(options);
                OrganizationModule.ConfigureMarten(options);
                IdentityModule.ConfigureMarten(options);

                options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
                
            })
            .AddAsyncDaemon(DaemonMode.HotCold)
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
            options.Discovery.IncludeAssembly(
                typeof(IdentityModule)
                    .Assembly);
            
            options.Policies.AutoApplyTransactions();
        }).StartAsync();
    }
}