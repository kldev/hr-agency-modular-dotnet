using System.Net;
using HrAgencySystem.Identity.Application.Users.ChangeRole;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

[Collection(IntegrationCollection.Name)]
public sealed class ChangeUserRoleEndpointTests(
    IntegrationEnvironment env,
    ITestOutputHelper output
) : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanUsers();
    }

    [Fact]
    public async Task ShouldChangeTheRole()
    {
        // Arrange
        var user = await UserClient.CreateAsync(
            _organizationId,
            email: "promoted@test.com",
            role: OrganizationRoleApi.Recruiter
        );

        // Act
        var result = await UserClient.ChangeRoleAsync(
            _organizationId,
            user.Id,
            OrganizationRoleApi.HiringManager
        );

        // Assert
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(_organizationId, result.OrganizationId);
        Assert.Equal(OrganizationRole.Recruiter, result.PreviousRole);
        Assert.Equal(OrganizationRole.HiringManager, result.Role);

        await Eventually.AssertAsync(
            async () =>
            {
                var projection = await UserClient.GetAsync(_organizationId, user.Id);

                Assert.Equal(OrganizationRole.HiringManager, projection.Role);
                Assert.NotNull(projection.ModifiedAt);
                Assert.NotNull(projection.ModifiedBy);
            },
            output: OutputHelper
        );
    }

    /// <summary>
    /// The second change has to compare against the role the first one left behind, not against the
    /// role the person was created with.
    /// </summary>
    [Fact]
    public async Task ShouldChangeTheRoleTwice()
    {
        var user = await UserClient.CreateAsync(
            _organizationId,
            email: "twice@test.com",
            role: OrganizationRoleApi.Recruiter
        );

        await UserClient.ChangeRoleAsync(_organizationId, user.Id, OrganizationRoleApi.Sales);

        var second = await UserClient.ChangeRoleAsync(
            _organizationId,
            user.Id,
            OrganizationRoleApi.Admin
        );

        Assert.Equal(OrganizationRole.Sales, second.PreviousRole);
        Assert.Equal(OrganizationRole.Admin, second.Role);
    }

    [Fact]
    public async Task ShouldNotChangeToTheRoleThePersonAlreadyHas()
    {
        var user = await UserClient.CreateAsync(
            _organizationId,
            email: "unchanged@test.com",
            role: OrganizationRoleApi.Sales
        );

        var response = await UserClient.ChangeRoleRawAsync(
            _organizationId,
            user.Id,
            OrganizationRoleApi.Sales
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            ChangeRoleHandler.SameRoleMessage,
            await response.Content.ReadAsStringAsync()
        );
    }

    [Fact]
    public async Task ShouldNotChangeTheRoleOfAUserFromAnotherOrganization()
    {
        var user = await UserClient.CreateAsync(_organizationId, email: "foreign@test.com");

        var response = await UserClient.ChangeRoleRawAsync(
            Guid.NewGuid(),
            user.Id,
            OrganizationRoleApi.Admin
        );

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
