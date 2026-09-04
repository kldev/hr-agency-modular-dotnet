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

    private Guid FirstJobDescriptionId { get; set; }
    private Guid SecondJobDescriptionId { get; set; }
    private Guid OtherOrganizationJobDescriptionId { get; set; }

    [Fact]
    public async Task ShouldGetStatusHistoryForAllScenarios()
    {
        await SetupJobDescriptions();

        await ShouldGetStatusHistoryFromOrganization();
        await ShouldGetStatusHistoryForSpecificJobDescription();
        await ShouldNotGetStatusHistoryFromAnotherOrganization();
        await ShouldReturnAllOrganizationHistoryWhenJobDescriptionIdIsEmpty();
        await ShouldReturnNoChangesWhenJobDescriptionHasNoStatusChanges();
        await ShouldGetAllStatusChangesForJobDescription();
    }

    private async Task SetupJobDescriptions()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        var first = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest());

        FirstJobDescriptionId = first.JobDescriptionId;

        Assert.Equal(OrganizationId, first.OrganizationId);

        var second = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest());

        SecondJobDescriptionId = second.JobDescriptionId;

        Assert.Equal(OrganizationId, second.OrganizationId);

        JobDescriptionClient.WithOrganizationId(OtherOrganizationId);

        var other = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest());

        OtherOrganizationJobDescriptionId = other.JobDescriptionId;

        Assert.Equal(OtherOrganizationId, other.OrganizationId);

        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await WaitForProjection();
    }

    private async Task ShouldGetStatusHistoryFromOrganization()
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

    private async Task ShouldGetStatusHistoryForSpecificJobDescription()
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
                x => Assert.Equal(
                    FirstJobDescriptionId,
                    x.JobDescriptionId));
        });
    }

    private async Task ShouldNotGetStatusHistoryFromAnotherOrganization()
    {
        JobDescriptionClient.WithOrganizationId(OtherOrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            OtherOrganizationJobDescriptionId,
            JobDescriptionStatus.Open);

        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Open);

        JobDescriptionClient.WithOrganizationId(OtherOrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory();

            Assert.Single(history);

            Assert.All(
                history,
                x => Assert.Equal(
                    OtherOrganizationJobDescriptionId,
                    x.JobDescriptionId));
        });
    }

    private async Task ShouldReturnAllOrganizationHistoryWhenJobDescriptionIdIsEmpty()
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

    private async Task ShouldReturnNoChangesWhenJobDescriptionHasNoStatusChanges()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        // FirstJobDescription already has status changes from previous scenarios,
        // therefore this scenario needs a fresh job description.
        var jobDescription = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest());

        await WaitForProjection();

        await Eventually.AssertAsync(async () =>
        {
            var history = await GetStatusHistory(jobDescription.JobDescriptionId);

            Assert.Single(history);
            Assert.Empty(history[0].Changes);
        });
    }

    private async Task ShouldGetAllStatusChangesForJobDescription()
    {
        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Open);

        await WaitForProjection();

        await JobDescriptionClient.ChangeStatusAsync(
            FirstJobDescriptionId,
            JobDescriptionStatus.Closed);

        await WaitForProjection();
        await Eventually.AssertAsync(
            async () =>
            {
                var history = await GetStatusHistory(FirstJobDescriptionId);

                Assert.Single(history);

                Assert.Equal(2, history[0].Changes.Count);
                Assert.Equal(
                    JobDescriptionStatus.Closed,
                    history[0].CurrentStatus);

                Assert.All(
                    history,
                    x => Assert.Equal(
                        FirstJobDescriptionId,
                        x.JobDescriptionId));
            },
            timeout: TimeSpan.FromSeconds(10),
            interval: TimeSpan.FromSeconds(2));
    }

    private async Task<IReadOnlyList<JdStatusChangeHistory>> GetStatusHistory(
        Guid? jobDescriptionId = null)
    {
        var response =
            await JobDescriptionClient.GetStatusHistoryAsync(jobDescriptionId);

        var result =
            await response.ReadWithJson<IReadOnlyList<JdStatusChangeHistory>>(
                OutputHelper);

        response.EnsureSuccessStatusCode();

        return result!;
    }

    private static Task WaitForProjection() =>
        Task.Delay(TimeSpan.FromSeconds(3));
}