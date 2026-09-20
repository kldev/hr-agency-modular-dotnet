using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Teams.Application.Suggestion;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Domain;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public sealed class GetTeamsTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTeams();
    }

    [Fact]
    public async Task ShouldSuggestTeamsOfTheOrganizationOnly()
    {
        await TeamClient.CreateAsync(
            _organizationId,
            "ShawSzenk",
            (Guid.NewGuid(), TeamRole.Sales)
        );
        await TeamClient.CreateAsync(Guid.NewGuid(), "Tiggers", (Guid.NewGuid(), TeamRole.Sales));

        await Eventually.AssertAsync(async () =>
        {
            var suggestions = await GetSuggestions();

            var only = Assert.Single(suggestions);
            Assert.Equal("ShawSzenk", only.Name);
            Assert.Equal(1, only.MemberCount);
        });
    }

    [Fact]
    public async Task ShouldFilterBySearch()
    {
        await TeamClient.CreateAsync(
            _organizationId,
            "ShawSzenk",
            (Guid.NewGuid(), TeamRole.Sales)
        );
        await TeamClient.CreateAsync(_organizationId, "Tiggers", (Guid.NewGuid(), TeamRole.Sales));

        await Eventually.AssertAsync(async () =>
        {
            var suggestions = await GetSuggestions("tigg");

            var only = Assert.Single(suggestions);
            Assert.Equal("Tiggers", only.Name);
        });
    }

    private async Task<IReadOnlyList<TeamSuggestion>> GetSuggestions(string? search = null)
    {
        Client.WithOrganizationId(_organizationId);

        var url = search is null
            ? "/api/suggestion/teams"
            : $"/api/suggestion/teams?search={Uri.EscapeDataString(search)}";

        var response = await Client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<IReadOnlyList<TeamSuggestion>>();

        Assert.NotNull(result);

        return result;
    }
}
