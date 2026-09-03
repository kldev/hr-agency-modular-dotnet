using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Projections;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobDescription;

[Collection(IntegrationCollection.Name)]
public sealed class StatusHistoryTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await SetupJobDescriptions();
    }

    private Guid FirstJobDescriptionId { get; set; }
    private Guid SecondJobDescriptionId { get; set; }

    private async Task SetupJobDescriptions()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        var first = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest());

        FirstJobDescriptionId = first.JobDescriptionId;
        Assert.Equal(OrganizationId, first.OrganizationId);

        var second = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest());
        Assert.Equal(OrganizationId, second.OrganizationId);

        SecondJobDescriptionId = second.JobDescriptionId;

        JobDescriptionClient.WithOrganizationId(OtherOrganizationId);

       var result = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest());
       
       Assert.Equal(OtherOrganizationId, result.OrganizationId);

        JobDescriptionClient.WithOrganizationId(OrganizationId);

        // wait for projection processed
        await Task.Delay(3000);
    }

    private async Task<IReadOnlyList<JdStatusChangeHistory>> GetStatusHistory(
        Guid? jobDescriptionId = null)
    {
        var response = await JobDescriptionClient.GetStatusHistoryAsync(jobDescriptionId);

        var result =
            await response.ReadWithJson<IReadOnlyList<JdStatusChangeHistory>>(OutputHelper);

        response.EnsureSuccessStatusCode();

        return result!;
    }

    [Fact]
    public async Task ShouldGetStatusHistoryFromOrganization()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Open);

        await JobDescriptionClient.ChangeStatusAsync(
            SecondJobDescriptionId,
            JobDescriptionStatus.Open);

        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory();

            Assert.Equal(2, history.Count);

            Assert.Contains(
                history,
                x => x.JobDescriptionId == FirstJobDescriptionId);

            Assert.Contains(
                history,
                x => x.JobDescriptionId == SecondJobDescriptionId);
        });
    }

    [Fact]
    public async Task ShouldGetStatusHistoryForSpecificJobDescription()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Open);

        await JobDescriptionClient.ChangeStatusAsync(
            SecondJobDescriptionId,
            JobDescriptionStatus.Cancelled);

        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory(FirstJobDescriptionId);

            Assert.Single(history);
            Assert.All(
                history,
                x => Assert.Equal(FirstJobDescriptionId, x.JobDescriptionId));
        });
    }

    [Fact]
    public async Task ShouldNotGetStatusHistoryFromAnotherOrganization()
    {
        var testOrganization = Guid.NewGuid();
        
        JobDescriptionClient.WithOrganizationId(testOrganization);

        var otherJobDescription =
            await JobDescriptionClient.CreateAsync(
                JobDescriptionTestData.CreateRequest());

        await Task.Delay(3000);

        await JobDescriptionClient.ChangeStatusAsync(
            otherJobDescription.JobDescriptionId,
            JobDescriptionStatus.Open);

        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Open);

        JobDescriptionClient.WithOrganizationId(testOrganization);

        
        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory();

            Assert.Single(history);
            Assert.All(
                history,
                x => Assert.Equal(
                    otherJobDescription.JobDescriptionId,
                    x.JobDescriptionId));
        });
    }

    [Fact]
    public async Task ShouldReturnAllOrganizationHistoryWhenJobDescriptionIdIsEmpty()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Open);

        await JobDescriptionClient.ChangeStatusAsync(
            SecondJobDescriptionId,
            JobDescriptionStatus.Open);

        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory(Guid.Empty);

            Assert.Equal(2, history.Count);

            Assert.Contains(
                history,
                x => x.JobDescriptionId == FirstJobDescriptionId);

            Assert.Contains(
                history,
                x => x.JobDescriptionId == SecondJobDescriptionId);
        });
    }

    [Fact]
    public async Task ShouldReturnNoChangesWhenJobDescriptionHasNoStatusChanges()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory(FirstJobDescriptionId);

            Assert.Single(history);
            Assert.Empty(history[0].Changes);
        });
    }

    [Fact]
    public async Task ShouldGetAllStatusChangesForJobDescription()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Open);
        await Task.Delay(TimeSpan.FromSeconds(3));
        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Closed);

        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory(FirstJobDescriptionId);

            Assert.Single(history);
            Assert.Equal(2, history[0].Changes.Count);
            Assert.Equal(JobDescriptionStatus.Closed, history[0].CurrentStatus);
            
            Assert.All(
                history,
                x => Assert.Equal(
                    FirstJobDescriptionId,
                    x.JobDescriptionId));

        }, timeout: TimeSpan.FromSeconds(10), interval: TimeSpan.FromSeconds(2));
    }
}