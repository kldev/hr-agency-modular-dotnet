using System.Net;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public sealed class GetUserByIdTests(IntegrationEnvironment environment, ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanUsers();
    }

    private async Task<HttpResponseMessage> GetSuggestion(Guid userId)
    {
        return await Client.GetAsync($"/api/suggestion/users/{userId}");
    }

    [Fact]
    public async Task ShouldGetUserSuggestionById()
    {
        var created = await UserClient.CreateAsync(OrganizationId, "recruiter@test.com",
            firstName: "Tom", lastName: "Moore", role: OrganizationRoleApi.Recruiter);

        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.Id);

            response.EnsureSuccessStatusCode();

            var suggestion = await response.ReadWithJson<UserSuggestion>(OutputHelper);

            Assert.NotNull(suggestion);
            Assert.Equal(created.Id, suggestion.Id);
            Assert.Equal("Tom Moore", suggestion.FullName);
            Assert.Equal("recruiter@test.com", suggestion.Email);
        });
    }

    [Fact]
    public async Task ShouldNotGetUserSuggestionFromAnotherOrganization()
    {
        var created = await UserClient.CreateAsync(OtherOrganizationId, "other@test.com");

        // wait until the projection is there, otherwise the 404 below would pass for the wrong reason
        Client.WithOrganizationId(OtherOrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.Id);

            response.EnsureSuccessStatusCode();
        });

        Client.WithOrganizationId(OrganizationId);

        var notFound = await GetSuggestion(created.Id);

        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);

        var problem = await notFound.ReadWithJson<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.NotFound, problem.Status);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownUserId()
    {
        Client.WithOrganizationId(OrganizationId);

        var response = await GetSuggestion(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
