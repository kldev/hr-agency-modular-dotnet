using HrAgencySystem.Api;
using HrAgencySystem.Company.Services;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.IntegrationTests.Infrastructure.Fakes;
using HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.ReportsService.Contracts;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wolverine;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public class ApiApplicationFactory(string connectionString) : WebApplicationFactory<IApiMarker>
{
    public TestLoggerProvider LoggerProvider { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", connectionString);

        builder.ConfigureServices(services =>
        {
            // The API wires RabbitMQ at startup and AutoProvisions the mail topology, which would
            // make every integration test depend on a running broker. Stubbing the external
            // transports keeps the outbox and the routing rules in place — a handler returning
            // OutgoingMessages still routes — while nothing ever leaves the process.
            services.DisableAllExternalWolverineTransports();

            services.Replace(
                ServiceDescriptor.Scoped<IOrganizationChecker, FakeOrganizationChecker>()
            );
            // Singleton so that a file uploaded in one request is still there in the next one.
            services.Replace(
                ServiceDescriptor.Singleton<IFileServiceClient, FakeFileServiceClient>()
            );
            // Singleton so a test can read back what the endpoint asked the reports service for.
            services.Replace(ServiceDescriptor.Singleton<IReportsClient, FakeReportsClient>());
            services.Replace(ServiceDescriptor.Scoped<IUserSnapshotRepository, FakeUserSnapshot>());
            services.Replace(
                ServiceDescriptor.Scoped<ICompanySnapshotRepository, FakeCompanySnapshot>()
            );
            services.Replace(
                ServiceDescriptor.Scoped<
                    IJobDescriptionSnapshotRepository,
                    FakeJobDescriptionSnapshot
                >()
            );
            services.Replace(
                ServiceDescriptor.Scoped<
                    IJobApplicationInfoQueryRepository,
                    FakeJobApplicationInfoQueryRepository
                >()
            );

            services.Replace(
                ServiceDescriptor.Scoped<
                    IOpportunitySnapshotRepository,
                    FakeSalesOpportunitySnapshot
                >()
            );

            services.Replace(ServiceDescriptor.Scoped<IRecruitmentService, FakeModuleService>());
            services.Replace(ServiceDescriptor.Scoped<ICompanyService, FakeModuleService>());
            services.Replace(ServiceDescriptor.Scoped<IJobDescriptionService, FakeModuleService>());
            services.Replace(ServiceDescriptor.Scoped<IIdentityService, FakeModuleService>());

            ConfigureAuthentication(services);
        });

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = connectionString,
                    }
                );
                builder.UseEnvironment("Testing");
            }
        );
        // builder.ConfigureLogging(logging =>
        // {
        //     logging.ClearProviders();
        //
        //     logging.AddProvider(LoggerProvider);
        //
        //     logging.SetMinimumLevel(LogLevel.Debug);
        // });
    }

    private void ConfigureAuthentication(IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme,
                _ => { }
            );
    }
}
