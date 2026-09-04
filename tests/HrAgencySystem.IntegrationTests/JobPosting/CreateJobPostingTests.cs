using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Events.JobPosting;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobPosting;

[Collection(IntegrationCollection.Name)]
public sealed class CreateJobPostingTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    [Fact]
    public async Task ShouldCreateJobPosting()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());

        var request = JobPostingTestData.CreateRequest();

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/recruitment/job-posting",
            request);

        // Assert
        var result = await response.ReadWithJson<JobPostCreated>(OutputHelper);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.JobPostId);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenRequestIsInvalid()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());

        var request = JobPostingTestData.CreateRequest() with
        {
            Title = JobPostingTestData.InvalidTitle
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/recruitment/job-posting",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}