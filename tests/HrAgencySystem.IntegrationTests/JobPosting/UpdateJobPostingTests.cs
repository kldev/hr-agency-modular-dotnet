using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Events.JobPosting;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobPosting;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateJobPostingTests(
    IntegrationEnvironment env,
    ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    [Fact]
    public async Task ShouldUpdateJobPosting()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());

        var createRequest = JobPostingTestData.CreateRequest();

        var created = await JobPostingClient.CreateAsync(
            createRequest);

        Assert.NotNull(created);

        var updateRequest = JobPostingTestData.UpdateRequest();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/recruitment/job-posting/{created.JobPostId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.ReadWithJson<JobPostUpdated>();

        Assert.NotNull(result);
        Assert.Equal(created.JobPostId, result.JobPostId);
        Assert.Equal(updateRequest.Title, result.Title);
        Assert.Equal(updateRequest.Summary, result.Summary);
        Assert.Equal(updateRequest.Description, result.Description);
        Assert.Equal(updateRequest.Location, result.Location);
        Assert.Equal(updateRequest.CountryCode, result.CountryCode);
        Assert.Equal(updateRequest.LanguageCode, result.LanguageCode);
        Assert.Equal(updateRequest.EmploymentType, result.EmploymentType);
        Assert.Equal(updateRequest.WorkMode, result.WorkMode);
        Assert.Equal(updateRequest.CurrencyCode, result.CurrencyCode);
        Assert.Equal(updateRequest.SalaryMin, result.SalaryMin);
        Assert.Equal(updateRequest.SalaryMax, result.SalaryMax);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await JobPostingClient.GetSingle(
                created.JobPostId);

            Assert.Equal(updateRequest.Title, projection.Title);
            Assert.Equal(updateRequest.Summary, projection.Summary);
            Assert.Equal(updateRequest.Description, projection.Description);
            Assert.Equal(updateRequest.Location, projection.Location);
            Assert.Equal(updateRequest.CountryCode, projection.CountryCode);
            Assert.Equal(updateRequest.LanguageCode, projection.LanguageCode);
            Assert.Equal(updateRequest.EmploymentType, projection.EmploymentType);
            Assert.Equal(updateRequest.WorkMode, projection.WorkMode);
            Assert.Equal(updateRequest.CurrencyCode, projection.CurrencyCode);
            Assert.Equal(updateRequest.SalaryMin, projection.SalaryMin);
            Assert.Equal(updateRequest.SalaryMax, projection.SalaryMax);
        });
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenRequestIsInvalid()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());

        var createRequest = JobPostingTestData.CreateRequest();

        var created = await JobPostingClient.CreateAsync(
            createRequest);

        Assert.NotNull(created);

        var updateRequest = JobPostingTestData.UpdateRequest() with
        {
            Title = string.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/recruitment/job-posting/{created.JobPostId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
