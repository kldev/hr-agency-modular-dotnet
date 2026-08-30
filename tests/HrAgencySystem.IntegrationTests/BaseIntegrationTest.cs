using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests;

public abstract class BaseIntegrationTest
{
    protected ITestOutputHelper OutputHelper { get; }

    protected BaseIntegrationTest(ApiPostgresTestContainer container, ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        var factory = new ApiApplicationFactory(container.ConnectionString);
        Client = factory.CreateClient();
    }

    protected HttpClient Client { get; }
}