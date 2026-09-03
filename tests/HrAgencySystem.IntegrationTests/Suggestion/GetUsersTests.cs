using HrAgencySystem.Identity.Application.Model;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public sealed class GetUsersTests : BaseIntegrationTest
{
    public GetUsersTests(IntegrationEnvironment environment, ITestOutputHelper output)
    : base(environment, output)
    {
        Cleaner.CleanUsers().Wait();
        SetupUsers().Wait();
    }
    
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    private async Task SetupUsers()
    {
        await UserClient.CreateAsync(
            OrganizationId,
            "first@test.com");

        await UserClient.CreateAsync(
            OrganizationId,
            "second@test.com");

        await UserClient.CreateAsync(
            OrganizationId,
            "sales@test.com", role: OrganizationRole.Sales, firstName: "Tom", lastName: "Moore");

        await UserClient.CreateAsync(
            OrganizationId,
            "recruiter@test.com", role: OrganizationRole.Recruiter);

        await UserClient.CreateAsync(
            OtherOrganizationId,
            "other@test.com");
    }

    private async Task<IReadOnlyList<UserSuggestion>> GetUserSuggestions(string search = "", IReadOnlyList<OrganizationRole>?roles = null)
    {
        var url = "/api/suggestion/users?search=" + search;
        if (roles?.Count > 0)
        {
            url += "&roles=" +
                   string.Join("&roles=", roles ?? []);
        }
        
        OutputHelper.WriteLine("url: " + url);
        var response = await Client.GetAsync(url);
        var result = (await response.ReadWithJson<IReadOnlyList<UserSuggestion>>(OutputHelper))!;
        response.EnsureSuccessStatusCode();
        return result;
    }

    [Fact]
    public async Task ShouldGetUsersFromOrganization()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var users = await GetUserSuggestions();

            Assert.Equal(4, users.Count);
            Assert.Contains(users, x => x.Email == "sales@test.com");
            Assert.Contains(users, x => x.Email == "recruiter@test.com");
            Assert.Contains(users, x => x.Email == "first@test.com");
            Assert.Contains(users, x => x.Email == "second@test.com");
            Assert.DoesNotContain(users, x => x.Email == "other@test.com");
        });
    }
    
    [Fact]
    public async Task ShouldGetUsersFilterBySearchQuery()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var users = await GetUserSuggestions(search: "tom");

            Assert.Single(users);
            Assert.Contains(users, x => x.Email == "sales@test.com");
    
        });
    }
    
    [Fact]
    public async Task ShouldGetUsersFilterByRolesQuery()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var users = await GetUserSuggestions(roles: [OrganizationRole.Sales, OrganizationRole.Recruiter]);

            Assert.Equal(2, users.Count);
            Assert.Contains(users, x => x.Email == "sales@test.com");
            Assert.Contains(users, x => x.Email == "recruiter@test.com");
        });
    }
}
