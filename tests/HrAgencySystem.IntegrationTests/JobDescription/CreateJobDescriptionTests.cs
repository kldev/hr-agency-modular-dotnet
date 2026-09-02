using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.JobDescription.Events;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobDescription;

[Collection(IntegrationCollection.Name)]
public sealed class CreateJobDescriptionTests(
    IntegrationEnvironment env,
    ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    [Fact]
    public async Task ShouldCreateJobDescription()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        Client
            .WithOrganizationId(organizationId);
        
        var request = JobDescriptionTestData.CreateRequest();

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/job-description",
            request);

        // Assert
        var result = await response.ReadWithJson<JobDescriptionCreated>(outputHelper);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

       

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.JobDescriptionId);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenRequestIsInvalid()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());
            

        var request = JobDescriptionTestData.CreateRequest() with
        {
            Title = JobDescriptionTestData.InvalidTitle
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/job-description",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}