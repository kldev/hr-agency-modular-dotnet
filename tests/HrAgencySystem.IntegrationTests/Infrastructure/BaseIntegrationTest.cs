using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest
{
    private readonly IntegrationEnvironment _environment;
    private readonly ITestOutputHelper _output;

    protected BaseIntegrationTest(IntegrationEnvironment environment, ITestOutputHelper output)
    {
        _environment = environment;
        _output = output;
        Client.AsOrganizationRoles();
    }

    protected HttpClient Client => _environment.Client;
    
    protected DatabaseCleaner Cleaner => _environment.Cleaner;
    protected ITestOutputHelper OutputHelper => _output;
    
}