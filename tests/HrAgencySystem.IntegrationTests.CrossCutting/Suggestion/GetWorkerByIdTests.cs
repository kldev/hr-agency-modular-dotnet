using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Workers.Application.Suggestion;
using HrAgencySystem.Workers.Domain;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public sealed class GetWorkerByIdTests(IntegrationEnvironment environment, ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanWorkers();
    }

    private async Task<HttpResponseMessage> GetSuggestion(Guid workerId) =>
        await Client.GetAsync($"/api/suggestion/workers/{workerId}");

    [Fact]
    public async Task ShouldGetWorkerSuggestionById()
    {
        var created = await WorkerClient.RegisterAsync(
            OrganizationId,
            firstName: "Oleksandr",
            lastName: "Tkachenko"
        );

        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.WorkerId);

            response.EnsureSuccessStatusCode();

            var suggestion = await response.ReadWithJson<WorkerSuggestion>(OutputHelper);

            Assert.NotNull(suggestion);
            Assert.Equal(created.WorkerId, suggestion.Id);
            Assert.Equal("Oleksandr Tkachenko", suggestion.FullName);
            Assert.Equal(WorkerStatus.Recruitment, suggestion.Status);
        });
    }

    [Fact]
    public async Task ShouldNotGetWorkerSuggestionFromAnotherOrganization()
    {
        var created = await WorkerClient.RegisterAsync(OtherOrganizationId);

        // Wait for the projection first, or the 404 below would pass for the wrong reason.
        Client.WithOrganizationId(OtherOrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.WorkerId);

            response.EnsureSuccessStatusCode();
        });

        Client.WithOrganizationId(OrganizationId);

        var notFound = await GetSuggestion(created.WorkerId);

        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);

        var problem = await notFound.ReadWithJson<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.NotFound, problem.Status);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownWorkerId()
    {
        Client.WithOrganizationId(OrganizationId);

        var response = await GetSuggestion(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
