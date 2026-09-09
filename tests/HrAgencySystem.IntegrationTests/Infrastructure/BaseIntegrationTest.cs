using HrAgencySystem.IntegrationTests.Candidates;
using HrAgencySystem.IntegrationTests.Companies;
using HrAgencySystem.IntegrationTests.Company;
using HrAgencySystem.IntegrationTests.Interviews;
using HrAgencySystem.IntegrationTests.JobDescriptions;
using HrAgencySystem.IntegrationTests.JobPosts;
using HrAgencySystem.IntegrationTests.SalesActivities;
using HrAgencySystem.IntegrationTests.SalesOpportunities;
using HrAgencySystem.IntegrationTests.Users;
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
        JobDescriptionClient = new JobDescriptionTestClient(environment.CreateClient().AsOrganizationRoles());
        UserClient = new UserTestClient(environment.CreateClient().AsOrganizationRoles(), OutputHelper);
        CompanyClient = new CompanyTestClient(_environment.CreateClient().AsOrganizationRoles());
        JobPostingClient = new JobPostingTestClient(_environment.CreateClient().AsOrganizationRoles());
        InterviewClient = new InterviewTestClient(_environment.CreateClient().AsOrganizationRoles(), output);
        CandidateClient = new CandidateTestClient(_environment.CreateClient().AsOrganizationRoles(), output);
        SalesActivityTestClient =
            new SalesActivityTestClient(_environment.CreateClient().AsOrganizationRoles(), output);

        OpportunityTestClient = new OpportunityTestClient(_environment.CreateClient().AsOrganizationRoles(), output);
     //   _environment.SetOutputHelper(output);

    }

    protected HttpClient Client => _environment.Client;
    
    protected DatabaseCleaner Cleaner => _environment.Cleaner;
    protected ITestOutputHelper OutputHelper { get; }

    protected JobDescriptionTestClient JobDescriptionClient { get; }
    protected UserTestClient UserClient { get; }
    protected CompanyTestClient CompanyClient { get; }
    protected JobPostingTestClient JobPostingClient { get; }
    protected InterviewTestClient InterviewClient { get; }
    protected CandidateTestClient CandidateClient { get; }
    
    protected SalesActivityTestClient SalesActivityTestClient { get; }
    
    protected OpportunityTestClient OpportunityTestClient { get;  }
    public async  Task InitializeAsync()
    {
        await BeforeEachAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void SetOutputHelper(ITestOutputHelper output)
    {
        _environment.SetOutputHelper(output);
    }
    
    protected virtual Task BeforeEachAsync() => Task.CompletedTask;

    public IServiceProvider Services => _environment.Services;
}