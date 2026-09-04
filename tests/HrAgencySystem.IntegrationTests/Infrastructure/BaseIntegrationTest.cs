using HrAgencySystem.IntegrationTests.Company;
using HrAgencySystem.IntegrationTests.JobDescription;
using HrAgencySystem.IntegrationTests.JobPosting;
using HrAgencySystem.IntegrationTests.User;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IntegrationEnvironment _environment;

    protected BaseIntegrationTest(IntegrationEnvironment environment, ITestOutputHelper output)
    {
        _environment = environment;
        OutputHelper = output;
        Client.AsOrganizationRoles();
        JobDescriptionClient = new(environment.CreateClient().AsOrganizationRoles());
        UserClient = new(environment.CreateClient().AsOrganizationRoles(), OutputHelper);
        CompanyClient = new(_environment.CreateClient().AsOrganizationRoles());
        JobPostingClient = new(_environment.CreateClient().AsOrganizationRoles());
    }

    protected HttpClient Client => _environment.Client;
    
    protected DatabaseCleaner Cleaner => _environment.Cleaner;
    protected ITestOutputHelper OutputHelper { get; }

    protected JobDescriptionTestClient JobDescriptionClient { get; }
    protected UserTestClient UserClient { get; }
    protected CompanyTestClient CompanyClient { get; }
    
    protected JobPostingTestClient JobPostingClient { get; }
    
    public async  Task InitializeAsync()
    {
        await BeforeEachAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task BeforeEachAsync() => Task.CompletedTask;
}