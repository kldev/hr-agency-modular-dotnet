using HrAgencySystem.Api;
using HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;
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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", connectionString);

        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped<IOrganizationChecker, FakeOrganizationChecker>());
            services.Replace(ServiceDescriptor.Scoped<IUserSnapshotRepository, FakeUserSnapshot>());
            services.Replace(ServiceDescriptor.Scoped<ICompanySnapshotRepository, FakeCompanySnapshot>());
            services.Replace(ServiceDescriptor.Scoped<IJobDescriptionSnapshotRepository, FakeJobDescriptionSnapshot>());
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