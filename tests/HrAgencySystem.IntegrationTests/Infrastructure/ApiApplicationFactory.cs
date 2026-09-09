using HrAgencySystem.Api;
using HrAgencySystem.Company.Services;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public class ApiApplicationFactory(string connectionString) : WebApplicationFactory<IApiMarker>
{
    public TestLoggerProvider LoggerProvider { get; } = new();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", connectionString);

        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped<IOrganizationChecker, FakeOrganizationChecker>());
            services.Replace(ServiceDescriptor.Scoped<IUserSnapshotRepository, FakeUserSnapshot>());
            services.Replace(ServiceDescriptor.Scoped<ICompanySnapshotRepository, FakeCompanySnapshot>());
            services.Replace(ServiceDescriptor.Scoped<IJobDescriptionSnapshotRepository, FakeJobDescriptionSnapshot>());
            services.Replace(ServiceDescriptor
                .Scoped<IJobApplicationInfoQueryRepository, FakeJobApplicationInfoQueryRepository>());

            services.Replace(
                ServiceDescriptor.Scoped<ISalesOpportunitySnapshotRepository, FakeSalesOpportunitySnapshot>());
            
            services.Replace(
                ServiceDescriptor.Scoped<ISalesService, FakeModuleService>());
            services.Replace(
                ServiceDescriptor.Scoped<IRecruitmentService, FakeModuleService>());
            services.Replace(
                ServiceDescriptor.Scoped<ICompanyService, FakeModuleService>());
            services.Replace(
                ServiceDescriptor.Scoped<IJobDescriptionService, FakeModuleService>());
            services.Replace(
                ServiceDescriptor.Scoped<IIdentityService, FakeModuleService>());
            
            
            ConfigureAuthentication(services);
        });
        
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = connectionString,
            });
            builder.UseEnvironment("Testing");
        });
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
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, opt => {});
    }
}