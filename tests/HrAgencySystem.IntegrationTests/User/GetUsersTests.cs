using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.User;

[Collection(IntegrationCollection.Name)]
public sealed class GetUsersTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output) : BaseIntegrationTest(environment, output)
{
    [Fact]
    public async Task Should_return_users_from_authenticated_user_organization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var otherOrganizationId = Guid.NewGuid();

        var firstUser = await UserClient.CreateAsync(
            organizationId,
            "first@test.com");

        var secondUser = await UserClient.CreateAsync(
            organizationId,
            "second@test.com");

        await UserClient.CreateAsync(
            otherOrganizationId,
            "other@test.com");

        Client.WithOrganizationId(organizationId);

        await Eventually.AssertAsync(
            async () =>
            {
                var response = await Client.GetAsync("/api/users");

                response.EnsureSuccessStatusCode();

                var users = (await response.ReadWithJson<List<UserProjection>>(output))!;
                
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

        Client.WithOrganizationId(organizationId);

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<List<UserProjection>>(
            output);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
