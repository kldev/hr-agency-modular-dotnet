using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.JobDescription.Events;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobDescription;

[Collection(IntegrationCollection.Name)]
public sealed class AssignRecruiterTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    [Fact]
    public async Task ShouldAssignRecruiter()
    {
        // Arrange
        Client
            .WithOrganizationId(Guid.NewGuid());
        
        var createRequest = JobDescriptionTestData.CreateRequest();

        var createResponse = await Client.PostAsJsonAsync(
            "/api/job-description",
            createRequest);
        
        var created = await createResponse.ReadWithJson<JobDescriptionCreated>(OutputHelper);
        createResponse.EnsureSuccessStatusCode();
        var recruiterId = Guid.NewGuid();

        var request = JobDescriptionTestData.CreateAssignRecruiterRequest(
            recruiterId);

        Assert.NotNull(created);
        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/job-description/{created.JobDescriptionId}/assign-recruiter",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.ReadWithJson<JobDescriptionRecruiterAssigned>();

        Assert.NotNull(result);
        Assert.Equal(recruiterId, result.Recruiter.Id);
    }
}