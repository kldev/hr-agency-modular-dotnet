using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.User;

[Collection(IntegrationCollection.Name)]
public sealed class CreateUserEndpointTests : BaseIntegrationTest
{
    public CreateUserEndpointTests(
        IntegrationEnvironment env,
        ITestOutputHelper outputHelper) : base(env, outputHelper)
    {
        Cleaner.CleanUserEmailReservation().Wait();
    }

    [Fact]
    public async Task ShouldCreateUser()
    {
        var organizationId = Guid.NewGuid();
        Client.WithOrganizationId(organizationId);
        
        // Arrange
        var request = new CreateUserRequest(
            
            Email: "user@test.com",
            FirstName: "John",
            LastName: "Doe",
            Role: OrganizationRole.Interviewer,
            Password: "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/users",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.ReadWithJson<UserProjection>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Id.ToString());
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.Equal(request.Role, result.Role);
    }

    [Fact]
    public async Task ShouldNotCreateTwoUsersWithTheSameEmail()
    {
        // Arrange
        var request = new CreateUserRequest(
            Email: "user@test.com",
            FirstName: "John",
            LastName: "Doe",
            Role: OrganizationRole.HiringManager,
            Password: "Password123!");

        // Act
        var firstResponse = await Client.PostAsJsonAsync(
            "/api/users",
            request);

        var secondResponse = await Client.PostAsJsonAsync(
            "/api/users",
            request with
            {
                FirstName = "Jane",
                LastName = "Smith",
                Password = "AnotherPassword123!"
            });

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task ShouldAllowCreatingUsersWithDifferentEmails()
    {
        // Arrange
        var organizationIdA = Guid.NewGuid();
        var organizationIdB = Guid.NewGuid();
        
        var firstRequest = new CreateUserRequest(
            Email: "user1@test.com",
            FirstName: "John",
            LastName: "Doe",
            Role: OrganizationRole.Sales,
            Password: "Password123!");

        var secondRequest = new CreateUserRequest(
            Email: "user2@test.com",
            FirstName: "Jane",
            LastName: "Smith",
            Role: OrganizationRole.Sales,
            Password: "Password123!");

        // Act
        Client.WithOrganizationId(organizationIdA);
        var firstResponse = await Client.PostAsJsonAsync(
            "/api/users",
            firstRequest);

        Client.WithOrganizationId(organizationIdB);
        var secondResponse = await Client.PostAsJsonAsync(
            "/api/users",
            secondRequest);

        // Assert
        firstResponse.EnsureSuccessStatusCode();
        secondResponse.EnsureSuccessStatusCode();

        var firstUser = await firstResponse.ReadWithJson<UserProjection>();
        var secondUser = await secondResponse.ReadWithJson<UserProjection>();

        Assert.NotNull(firstUser);
        Assert.NotNull(secondUser);
        Assert.NotEqual(firstUser.Id, secondUser.Id);

        Assert.Equal(organizationIdA, firstUser.OrganizationId);
        Assert.Equal(organizationIdB, secondUser.OrganizationId);

        Assert.Equal(firstRequest.Email, firstUser.Email);
        Assert.Equal(secondRequest.Email, secondUser.Email);

        Assert.Equal(firstRequest.FirstName, firstUser.FirstName);
        Assert.Equal(secondRequest.FirstName, secondUser.FirstName);

        Assert.Equal(firstRequest.LastName, firstUser.LastName);
        Assert.Equal(secondRequest.LastName, secondUser.LastName);

        Assert.Equal(firstRequest.Role, firstUser.Role);
        Assert.Equal(secondRequest.Role, secondUser.Role);
    }
}