using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Application.Suggestion;
using HrAgencySystem.Projects.Domain;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public sealed class GetProjectByIdTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output
) : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
    }

    private async Task<HttpResponseMessage> GetSuggestion(Guid projectId) =>
        await Client.GetAsync($"/api/suggestion/projects/{projectId}");

    [Fact]
    public async Task ShouldGetProjectSuggestionById()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(OrganizationId);
        var created = await ProjectClient.CreateAsync(OrganizationId, companyId);

        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.ProjectId);

            response.EnsureSuccessStatusCode();

            var suggestion = await response.ReadWithJson<ProjectSuggestion>(OutputHelper);

            Assert.NotNull(suggestion);
            Assert.Equal(created.ProjectId, suggestion.Id);
            Assert.Equal(created.Name, suggestion.Name);
            Assert.Equal(ProjectStatus.Draft, suggestion.Status);
        });
    }

    [Fact]
    public async Task ShouldNotGetProjectSuggestionFromAnotherOrganization()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(OtherOrganizationId);
        var created = await ProjectClient.CreateAsync(OtherOrganizationId, companyId);

        // Wait for the projection first, or the 404 below would pass for the wrong reason.
        Client.WithOrganizationId(OtherOrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.ProjectId);

            response.EnsureSuccessStatusCode();
        });

        Client.WithOrganizationId(OrganizationId);

        var notFound = await GetSuggestion(created.ProjectId);

        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);

        var problem = await notFound.ReadWithJson<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.NotFound, problem.Status);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownProjectId()
    {
        Client.WithOrganizationId(OrganizationId);

        var response = await GetSuggestion(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
