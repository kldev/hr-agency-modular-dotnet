using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.JobDescription.Domain;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobDescription;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateJobDescriptionStatusTests(
    IntegrationEnvironment env,
    ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    [Fact]
    public async Task ShouldUpdateJobDescriptionStatus()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());
        Client.AsOrganizationRoles();
        
        var createRequest = JobDescriptionTestData.CreateRequest();

        var created = await JobDescriptionClient.CreateAsync(createRequest);

        Assert.NotNull(created);
        
        // Act
        var result = await JobDescriptionClient.ChangeStatusAsync(
            created.JobDescriptionId,
            JobDescriptionStatus.Open);

        // Assert
        Assert.Equal(JobDescriptionStatus.Open, result.Status);

        await Task.Delay(1000);
        // Act
        var resultClosed =  await JobDescriptionClient.ChangeStatusAsync(
            created.JobDescriptionId,
            JobDescriptionStatus.Closed);
        
        Assert.Equal(JobDescriptionStatus.Closed, resultClosed.Status);
        
        await Eventually.AssertAsync(async () =>
        {
            var projection = await JobDescriptionClient.GetSingle(created.JobDescriptionId);
            Assert.Equal(JobDescriptionStatus.Closed, projection.Status);
        });
    }
}