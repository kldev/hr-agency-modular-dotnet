using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Events.JobPostings;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobPosting;

[Collection(IntegrationCollection.Name)]
public sealed class ChangeJobPostingRecruiterTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    [Fact]
    public async Task ShouldChangeJobPostingRecruiter()
    {
        var organizationId = Guid.NewGuid();
        // Arrange
        Client
            .WithOrganizationId(organizationId);

        JobPostingClient.WithOrganizationId(organizationId);
        
        var createRequest = JobPostingTestData.CreateRequest();

        var created = await JobPostingClient.CreateAsync(
            createRequest);

        Assert.NotNull(created);
        Assert.Equal(organizationId, created.OrganizationId);

        var recruiterId = Guid.NewGuid();

        var request = new AssignRecruiter(recruiterId);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/recruitment/job-posting/{created.JobPostId}/change-recruiter",
            request);

        // Assert
        var result = await response.ReadWithJson<JobPostRecruiterChanged>(OutputHelper);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

  

        Assert.NotNull(result);
        Assert.Equal(created.JobPostId, result.JobPostId);
        Assert.Equal(recruiterId, result.Recruiter.Id);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await JobPostingClient.GetSingle(
                created.JobPostId);

            Assert.Equal(recruiterId, projection.RecruiterId);
        });
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenRecruiterIdIsEmpty()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());

        var createRequest = JobPostingTestData.CreateRequest();

        var created = await JobPostingClient.CreateAsync(
            createRequest);

        Assert.NotNull(created);

        var request = new AssignRecruiter(
            Guid.Empty);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/recruitment/job-posting/{created.JobPostId}/change-recruiter",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
