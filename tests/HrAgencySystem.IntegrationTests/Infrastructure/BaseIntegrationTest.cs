using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest(IntegrationEnvironment environment, ITestOutputHelper output)
{
    protected HttpClient Client  => environment.Client;
    protected DatabaseCleaner Cleaner => environment.Cleaner;
    protected ITestOutputHelper OutputHelper => output;
}