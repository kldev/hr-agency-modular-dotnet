using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

[Collection(IntegrationCollection.Name)]
public sealed class ChangeUserPasswordEndpointTests : BaseIntegrationTest
{
    private const string CurrentPassword = "Password123!";

    public ChangeUserPasswordEndpointTests(
        IntegrationEnvironment env,
        ITestOutputHelper outputHelper
    )
        : base(env, outputHelper)
    {
        Cleaner.CleanUsers().Wait();
    }

    [Fact]
    public async Task ShouldChangeOwnPassword()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "user@test.com");

        AuthenticateAs(organizationId, user.Id);

        // Act
        var response = await ChangePassword(CurrentPassword, "NewPassword123!");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // the new password is the one stored now, so it unlocks the next change
        var second = await ChangePassword("NewPassword123!", "EvenNewerPassword123!");

        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task ShouldNotChangePasswordWithWrongCurrentPassword()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "wrong@test.com");

        AuthenticateAs(organizationId, user.Id);

        // Act
        var response = await ChangePassword("NotMyPassword123!", "NewPassword123!");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // the old password still works
        var retry = await ChangePassword(CurrentPassword, "NewPassword123!");

        Assert.Equal(HttpStatusCode.NoContent, retry.StatusCode);
    }

    [Fact]
    public async Task ShouldNotChangePasswordToTheSameOne()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "same@test.com");

        AuthenticateAs(organizationId, user.Id);

        // Act
        var response = await ChangePassword(CurrentPassword, CurrentPassword);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldValidatePasswordPolicy()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "policy@test.com");

        AuthenticateAs(organizationId, user.Id);

        // Act
        var response = await ChangePassword(CurrentPassword, "abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotChangePasswordOfUserFromAnotherOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var otherOrganizationId = Guid.NewGuid();

        var user = await UserClient.CreateAsync(organizationId, email: "victim@test.com");

        // the caller authenticates with somebody else's user id, but their own organization
        AuthenticateAs(otherOrganizationId, user.Id);

        // Act
        var response = await ChangePassword(CurrentPassword, "NewPassword123!");

        // Assert
        Assert.NotEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    private void AuthenticateAs(Guid organizationId, Guid userId)
    {
        Client.WithOrganizationId(organizationId);
        Client.WithUserId(userId);
    }

    private async Task<HttpResponseMessage> ChangePassword(
        string currentPassword,
        string newPassword
    )
    {
        return await Client.PutAsJsonAsync(
            "/api/users/me/password",
            new ChangePasswordRequest(currentPassword, newPassword)
        );
    }
}
