using HrAgencySystem.Api;
using HrAgencySystem.Organization.Infrastructure;
using HrAgencySystem.SharedKernel.Port;
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
        Environment.SetEnvironmentVariable("Application:DisableChecker", "True");
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", connectionString);

        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped<IOrganizationChecker, FakeOrganizationChecker>());
        });
        
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = connectionString,
                ["AllowFixedId"] = "0",
            });
            builder.UseEnvironment("Testing");
        });
    }
}