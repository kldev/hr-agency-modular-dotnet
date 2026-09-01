using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.User;

[Collection(IntegrationCollection.Name)]
public sealed class GetUsersTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output)
{
    private readonly HttpClient _client = environment.CreateClient();
    private readonly UserTestClient _users = new(
        environment.CreateClient(),
        output);

    [Fact]
    public async Task Should_return_users_from_authenticated_user_organization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var otherOrganizationId = Guid.NewGuid();

        var firstUser = await _users.CreateAsync(
            organizationId,
            "first@test.com");

        var secondUser = await _users.CreateAsync(
            organizationId,
            "second@test.com");

        await _users.CreateAsync(
            otherOrganizationId,
            "other@test.com");

        _client.WithOrganizationId(organizationId);

        await Eventually.AssertAsync(
            async () =>
            {
                var response = await _client.GetAsync("/api/users");

                response.EnsureSuccessStatusCode();

                var users = await response.ReadWithJson<List<UserProjection>>(output);

                Assert.Contains(users, x => x.Id == firstUser.Id);
                Assert.Contains(users, x => x.Id == secondUser.Id);
                Assert.DoesNotContain(users, x => x.Email == "other@test.com");
            });
    }

    [Fact]
    public async Task Should_return_empty_collection_when_organization_has_no_users()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        _client.WithOrganizationId(organizationId);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<List<UserProjection>>(
            output);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
