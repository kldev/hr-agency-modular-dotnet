using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Organization.Maps;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Users;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Organization;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateOrganizationUserTests : BaseIntegrationTest
{
    public UpdateOrganizationUserTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
        : base(env, outputHelper)
    {
        Cleaner.CleanUsers().Wait();
        Client.AsOwner();
    }

    [Fact]
    public async Task ShouldUpdateUserOfAnyOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "before@test.com");

        var request = new UpdateUserForOrganizationRequest(
            OrganizationId: organizationId,
            Email: "after@test.com",
            FirstName: "Johnny",
            LastName: "Bravo",
            JobTitle: "Lead Recruiter",
            Phone: "+48 600 100 200"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/organization/users/{user.Id}", request);

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

        // reading the projection back is an organization-scoped call, not an owner one
        Client.AsOrganizationRoles();
        Client.WithOrganizationId(organizationId);

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
                Assert.NotNull(projection.ModifiedAt);
            },
            output: OutputHelper
        );
    }

    [Fact]
    public async Task ShouldNotUpdateUserFromAnotherOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "user@test.com");

        var request = new UpdateUserForOrganizationRequest(
            OrganizationId: Guid.NewGuid(),
            Email: "hijacked@test.com",
            FirstName: "John",
            LastName: "Doe"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/organization/users/{user.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotUseEmailOfAnotherUserInTheSameOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "first@test.com");
        await UserClient.CreateAsync(organizationId, email: "second@test.com");

        var request = new UpdateUserForOrganizationRequest(
            OrganizationId: organizationId,
            Email: "second@test.com",
            FirstName: "John",
            LastName: "Doe"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/organization/users/{user.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturn403ForOrganizationRoles()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, email: "roles@test.com");

        Client.AsOrganizationRoles();

        var request = new UpdateUserForOrganizationRequest(
            OrganizationId: organizationId,
            Email: "after@test.com",
            FirstName: "John",
            LastName: "Doe"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/organization/users/{user.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
