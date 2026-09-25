using HrAgencySystem.Agency;
using HrAgencySystem.Company;
using HrAgencySystem.EmailTemplates.Messaging;
using HrAgencySystem.Forms;
using HrAgencySystem.Identity;
using HrAgencySystem.JobDescription;
using HrAgencySystem.LegalEntities;
using HrAgencySystem.Organization;
using HrAgencySystem.Projects;
using HrAgencySystem.Recruitment;
using HrAgencySystem.Sales;
using HrAgencySystem.Tasks;
using HrAgencySystem.Teams;
using HrAgencySystem.Workers;
using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using Marten;
using JasperFx.OpenTelemetry;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Api.Infrastructure;

public static class SetupMartenExtensions
{
    public static void SetupMartenForApplication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddMarten(options =>
            {
                var connectionString = configuration.GetConnectionString("Postgres");
                options.Connection(connectionString!);

                options.Events.DatabaseSchemaName = "events";
                options.Events.StreamIdentity = StreamIdentity.AsGuid;

                ConfigureModules(options);

                // Spans for opening a connection and failing on one, and a counter per appended
                // event type - the rest (every SQL command) comes from the Npgsql instrumentation.
                options.OpenTelemetry.TrackConnections = TrackLevel.Normal;
                options.OpenTelemetry.TrackEventCounters();

                options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            })
            .AddAsyncDaemon(DaemonMode.HotCold)
            .IntegrateWithWolverine(x =>
            {
                x.MessageStorageSchemaName = "messages";
            });
    }

    private static void ConfigureModules(StoreOptions options)
    {
        CompanyModule.ConfigureMarten(options);
        OrganizationModule.ConfigureMarten(options);
        IdentityModule.ConfigureMarten(options);
        JobDescriptionModule.ConfigureMarten(options);
        RecruitmentModule.ConfigureMarten(options);
        AgencyModule.ConfigureMarten(options);
        ProjectsModule.ConfigureMarten(options);
        WorkersModule.ConfigureMarten(options);
        FormsModule.ConfigureMarten(options);
        SalesModule.ConfigureMarten(options);
        TasksModule.ConfigureMarten(options);
        TeamsModule.ConfigureMarten(options);
        LegalEntitiesModule.ConfigureMarten(options);
    }

    public static void SetupWolverineForApplication(
        this ConfigureHostBuilder builder,
        IConfiguration configuration
    )
    {
        var section = configuration.GetSection(RabbitMqConfig.SectionName);
        var config = RabbitMqConfig.FromSection(section);
        builder
            .UseWolverine(options =>
            {
                options.PublishEmailMessages(config);
                ConfigureDiscover(options);
                IdentityModule.ConfigureWolverine(options);
                TeamsModule.ConfigureWolverine(options);
                SalesModule.ConfigureWolverine(options);

                options.Policies.AutoApplyTransactions();
            })
            .StartAsync();
    }

    private static void ConfigureDiscover(WolverineOptions options)
    {
        options.Discovery.IncludeAssembly(typeof(CompanyModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(OrganizationModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(IdentityModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(JobDescriptionModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(RecruitmentModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(AgencyModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(ProjectsModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(WorkersModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(FormsModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(SalesModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(TasksModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(TeamsModule).Assembly);
        options.Discovery.IncludeAssembly(typeof(LegalEntitiesModule).Assembly);
    }
}
