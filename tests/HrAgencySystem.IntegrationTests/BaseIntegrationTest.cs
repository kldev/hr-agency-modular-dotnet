using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.SharedKernel.Port;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests;

public abstract class BaseIntegrationTest
{
    protected ITestOutputHelper OutputHelper { get; }

    protected BaseIntegrationTest(ApiPostgresTestContainer container, ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        var factory = new ApiApplicationFactory(container.ConnectionString).WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var service = services.SingleOrDefault(z => z.ServiceType == typeof(IOrganizationChecker));
                outputHelper.WriteLine("Using IOrganizationChecker factory: " + service!.GetType().AssemblyQualifiedName);
            });
        });
        factory.StartServer();
        Client = factory.CreateClient();
    }

    
    
    protected HttpClient Client { get; }
}