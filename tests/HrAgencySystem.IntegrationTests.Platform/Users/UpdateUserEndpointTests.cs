using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateUserEndpointTests : BaseIntegrationTest
{
    public UpdateUserEndpointTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
        : base(env, outputHelper)
    {
        Cleaner.CleanUsers().Wait();
    }

    [Fact]
    public async Task ShouldUpdateUser()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "before@test.com");

        Client.WithOrganizationId(organizationId);

        var request = new UpdateUserRequest(
            Email: "after@test.com",
            FirstName: "Johnny",
            LastName: "Bravo",
            JobTitle: "Lead Recruiter",
            Phone: "+48 600 100 200"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{user.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.ReadWithJson<UserUpdated>(OutputHelper);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal("after@test.com", result.Contact.Email);
        Assert.Equal("Johnny", result.Contact.FirstName);
        Assert.Equal("Bravo", result.Contact.LastName);
        Assert.Equal("Lead Recruiter", result.Contact.JobTitle);

        await Eventually.AssertAsync(
            async () =>
            {
                var projectionResponse = await Client.GetAsync($"/api/users/{user.Id}");

                projectionResponse.EnsureSuccessStatusCode();

                var projection = await projectionResponse.ReadWithJson<UserProjection>();

                Assert.NotNull(projection);
                Assert.Equal("after@test.com", projection.Email);
                Assert.Equal("Johnny", projection.FirstName);
                Assert.Equal("Bravo", projection.LastName);
                Assert.Equal("Lead Recruiter", projection.JobTitle);
                Assert.NotNull(projection.ModifiedAt);
            },
            output: OutputHelper
        );
    }

    [Fact]
    public async Task ShouldReleaseThePreviousEmailAfterChange()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "old@test.com");

        Client.WithOrganizationId(organizationId);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/users/{user.Id}",
            new UpdateUserRequest("new@test.com", "John", "Doe")
        );

        response.EnsureSuccessStatusCode();

        // Assert - the freed address can be taken by somebody else
        var reused = await UserClient.CreateAsync(organizationId, email: "old@test.com");

        Assert.NotEqual(user.Id, reused.Id);
    }

    [Fact]
    public async Task ShouldNotUseEmailOfAnotherUserInTheSameOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "first@test.com");
        await UserClient.CreateAsync(organizationId, email: "second@test.com");

        Client.WithOrganizationId(organizationId);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/users/{user.Id}",
            new UpdateUserRequest("second@test.com", "John", "Doe")
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotUpdateUserFromAnotherOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var otherOrganizationId = Guid.NewGuid();

        var user = await UserClient.CreateAsync(organizationId, email: "owner@test.com");

        Client.WithOrganizationId(otherOrganizationId);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/users/{user.Id}",
            new UpdateUserRequest("hijacked@test.com", "John", "Doe")
        );

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ShouldValidateContactData()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "valid@test.com");

        Client.WithOrganizationId(organizationId);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/users/{user.Id}",
            new UpdateUserRequest("", "", "")
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
