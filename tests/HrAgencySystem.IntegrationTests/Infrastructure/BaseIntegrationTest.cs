using HrAgencySystem.IntegrationTests.JobDescription;
using HrAgencySystem.IntegrationTests.User;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest
{
    private readonly IntegrationEnvironment _environment;

    protected BaseIntegrationTest(IntegrationEnvironment environment, ITestOutputHelper output)
    {
        _environment = environment;
        OutputHelper = output;
        Client.AsOrganizationRoles();
    }

    protected HttpClient Client => _environment.Client;
    
    protected DatabaseCleaner Cleaner => _environment.Cleaner;
    protected ITestOutputHelper OutputHelper { get; }

    protected JobDescriptionTestClient JobDescriptionClient =>
        new(_environment.CreateClient().AsOrganizationRoles());
    
    protected UserTestClient UserClient =>
        new(_environment.CreateClient().AsOrganizationRoles(), OutputHelper);
    
}