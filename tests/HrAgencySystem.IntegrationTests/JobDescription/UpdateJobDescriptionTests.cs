using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Projections;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobDescription;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateJobDescriptionTests(
    IntegrationEnvironment env,
    ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    [Fact]
    public async Task ShouldUpdateJobDescription()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        JobDescriptionClient
            .WithOrganizationId(organizationId);
        
        Client.WithOrganizationId(organizationId);

        var createRequest = JobDescriptionTestData.CreateRequest();

        var created = await JobDescriptionClient.CreateAsync(
            createRequest);
        
        Assert.NotNull(created);

        var updateRequest = JobDescriptionTestData.UpdateRequest();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/job-description/{created.JobDescriptionId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.ReadWithJson<JobDescriptionUpdated>();

        Assert.NotNull(result);
        Assert.Equal(result.SalaryMin, updateRequest.SalaryMin);
        Assert.Equal(result.SalaryMax, updateRequest.SalaryMax);
        Assert.Equal(result.WorkMode, updateRequest.WorkMode);
        Assert.Equal(result.Summary, updateRequest.Summary);
        Assert.Equal(result.Description, updateRequest.Description);


        await Eventually.AssertAsync(async () =>
        {
            var projection = await JobDescriptionClient.GetSingle(created.JobDescriptionId);

            Assert.Equal(projection.SalaryMin, updateRequest.SalaryMin);
            Assert.Equal(projection.SalaryMax, updateRequest.SalaryMax);
            Assert.Equal(projection.WorkMode, updateRequest.WorkMode);
            Assert.Equal(projection.Summary, updateRequest.Summary);
            Assert.Equal(projection.Description, updateRequest.Description);
        });
    }
}