using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Owner;

[Collection(IntegrationCollection.Name)]
public sealed class CreateOwnerEndpointTests : BaseIntegrationTest
{
    public CreateOwnerEndpointTests(IntegrationEnvironment env, ITestOutputHelper outputHelper) : base(env, outputHelper) 
    {
        Cleaner.CleanOwnerEmailReservation().Wait();
    }
    
    [Fact]
    public async Task ShouldCreateOwner()
    {
        // Arrange
        var request = new CreatePlatformOwner(
            Email: "owner@test.com",
            Password: "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/owners",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.ReadWithJson<OwnerProjection>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Id.ToString());
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(PlatformRole.Owner, result.Role);
    }

    [Fact]
    public async Task ShouldNotCreateTwoOwnersWithTheSameEmail()
    {
        // Arrange
        var request = new CreatePlatformOwner(
            Email: "owner@test.com",
            Password: "Password123!");

        // Act
        var firstResponse = await Client.PostAsJsonAsync(
            "/api/owners",
            request);

        var secondResponse = await Client.PostAsJsonAsync(
            "/api/owners",
            request with
            {
                Password = "AnotherPassword123!"
            });

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

    }

    [Fact]
    public async Task ShouldAllowCreatingOwnersWithDifferentEmails()
    {
        // Arrange
        var firstRequest = new CreatePlatformOwner(
            Email: "owner1@test.com",
            Password: "Password123!");

        var secondRequest = new CreatePlatformOwner(
            Email: "owner2@test.com",
            Password: "Password123!");

        // Act
        var firstResponse = await Client.PostAsJsonAsync(
            "/api/owners",
            firstRequest);

        var secondResponse = await Client.PostAsJsonAsync(
            "/api/owners",
            secondRequest);

        // Assert
        firstResponse.EnsureSuccessStatusCode();
        secondResponse.EnsureSuccessStatusCode();

        var firstOwner = await firstResponse.ReadWithJson<OwnerProjection>();

        var secondOwner = await secondResponse.ReadWithJson<OwnerProjection>();

        Assert.NotNull(firstOwner);
        Assert.NotNull(secondOwner);
        Assert.NotEqual(firstOwner.Id, secondOwner.Id);
        
        Assert.Equal(firstRequest.Email, firstOwner.Email);
        Assert.Equal(secondRequest.Email, secondOwner.Email);
        
    }

    [Fact]
    public async Task ShouldReturn403ForAuthenticatedUserWithOrganizationsRoles()
    {
        Client.AsOrganizationRoles();

        var request = new CreatePlatformOwner(
            Email: "owner@test.com",
            Password: "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/owners",
            request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task WithOwnerRoleShouldCreateOwner()
    {
        Client.AsOwner();

        var request = new CreatePlatformOwner(
            Email: "otherOwner@test.com",
            Password: "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/owners",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

}