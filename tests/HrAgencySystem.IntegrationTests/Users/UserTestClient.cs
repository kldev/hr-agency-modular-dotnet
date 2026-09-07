using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

public sealed class UserTestClient(
    HttpClient client,
    ITestOutputHelper output)
{
    public async Task<UserProjection> CreateAsync(
        Guid organizationId,
        string email = "user@test.com",
        string firstName = "John",
        string lastName = "Doe",
        OrganizationRoleApi role = OrganizationRoleApi.Admin,
        string password = "Password123!")
    {
        var request = new CreateUserRequest(
            email,
            firstName,
            lastName,
            role,
            password);

        client.WithOrganizationId(organizationId);
        var response = await client.PostAsJsonAsync(
            "/api/users",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<UserProjection>(
            output);

        Assert.NotNull(result);

        return result;
    }
}