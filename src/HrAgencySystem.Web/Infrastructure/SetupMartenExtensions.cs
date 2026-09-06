using HrAgencySystem.Company;
using HrAgencySystem.Organization;
using HrAgencySystem.Recruitment;
using JasperFx;
using JasperFx.Events;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Web.Infrastructure;

public static class SetupMartenExtensions
{
    public static void SetupMartenForApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(options =>
            {
                var connectionString = configuration.GetConnectionString("Postgres");

                options.Connection(connectionString!);

                options.Events.DatabaseSchemaName = "events";
                options.Events.StreamIdentity =
                    StreamIdentity.AsGuid;
                RecruitmentModule.ConfigureMartenMinimal(options);
                CompanyModule.ConfigureMartenMinimal(options);
                OrganizationModule.ConfigureMarten(options);
                
                options.AutoCreateSchemaObjects = AutoCreate.None;
            })
            .IntegrateWithWolverine();
    }

    public static void SetupWolverineForApplication(this ConfigureHostBuilder builder)
    {
        builder.UseWolverine(options =>
        {
            options.Discovery.IncludeAssembly(
                typeof(RecruitmentModule)
                    .Assembly);

            options.Policies.AutoApplyTransactions();
        });
    }
}