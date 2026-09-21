using HrAgencySystem.IntegrationTests.Agency;
using HrAgencySystem.IntegrationTests.Candidates;
using HrAgencySystem.IntegrationTests.Companies;
using HrAgencySystem.IntegrationTests.Company;
using HrAgencySystem.IntegrationTests.Interviews;
using HrAgencySystem.IntegrationTests.JobDescriptions;
using HrAgencySystem.IntegrationTests.JobPosts;
using HrAgencySystem.IntegrationTests.Projects;
using HrAgencySystem.IntegrationTests.SalesActivities;
using HrAgencySystem.IntegrationTests.SalesFollowUpActions;
using HrAgencySystem.IntegrationTests.SalesOpportunities;
using HrAgencySystem.IntegrationTests.Teams;
using HrAgencySystem.IntegrationTests.Users;
using HrAgencySystem.IntegrationTests.Workers;
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
        JobDescriptionClient = new JobDescriptionTestClient(
            environment.CreateClient().AsOrganizationRoles()
        );
        UserClient = new UserTestClient(
            environment.CreateClient().AsOrganizationRoles(),
            OutputHelper
        );
        CompanyClient = new CompanyTestClient(_environment.CreateClient().AsOrganizationRoles());
        JobPostingClient = new JobPostingTestClient(
            _environment.CreateClient().AsOrganizationRoles()
        );
        InterviewClient = new InterviewTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );
        CandidateClient = new CandidateTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );
        SalesActivityTestClient = new SalesActivityTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );

        OpportunityTestClient = new OpportunityTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );
        FollowUpActionTestClient = new FollowUpActionTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );
        ProjectClient = new ProjectTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );
        OrgStructureClient = new OrgStructureTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );
        TeamClient = new TeamTestClient(_environment.CreateClient().AsOrganizationRoles(), output);
        WorkerClient = new WorkerTestClient(
            _environment.CreateClient().AsOrganizationRoles(),
            output
        );
        //   _environment.SetOutputHelper(output);
    }

    protected HttpClient Client => _environment.Client;

    protected DatabaseCleaner Cleaner => _environment.Cleaner;

    /// <summary>For the rare test that needs a client with different credentials than its own.</summary>
    protected IntegrationEnvironment Env => _environment;
    protected WorkerTestClient WorkerClient { get; }
    protected ITestOutputHelper OutputHelper { get; }

    protected JobDescriptionTestClient JobDescriptionClient { get; }
    protected UserTestClient UserClient { get; }
    protected CompanyTestClient CompanyClient { get; }
    protected JobPostingTestClient JobPostingClient { get; }
    protected InterviewTestClient InterviewClient { get; }
    protected CandidateTestClient CandidateClient { get; }

    protected SalesActivityTestClient SalesActivityTestClient { get; }

    protected OpportunityTestClient OpportunityTestClient { get; }

    protected FollowUpActionTestClient FollowUpActionTestClient { get; }

    protected TeamTestClient TeamClient { get; }

    protected ProjectTestClient ProjectClient { get; }

    protected OrgStructureTestClient OrgStructureClient { get; }

    public async Task InitializeAsync()
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
