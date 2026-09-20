using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Teams.Contracts;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

public sealed class UserTestClient(HttpClient client, ITestOutputHelper output)
{
    public async Task<UserProjection> CreateAsync(
        Guid organizationId,
        string email = "user@test.com",
        string firstName = "John",
        string lastName = "Doe",
        OrganizationRoleApi role = OrganizationRoleApi.Admin,
        string password = "Password123!",
        Guid? teamId = null,
        TeamRole? teamRole = null
    )
    {
        var request = new CreateUserRequest(
            email,
            firstName,
            lastName,
            role,
            password,
            TeamId: teamId,
            TeamRole: teamRole
        );

        client.WithOrganizationId(organizationId);
        var response = await client.PostAsJsonAsync("/api/users", request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<UserProjection>(output);

        Assert.NotNull(result);

        return result;
    }

    public async Task<HttpResponseMessage> CreateRawAsync(
        Guid organizationId,
        string email,
        Guid? teamId = null,
        TeamRole? teamRole = null
    )
    {
        var request = new CreateUserRequest(
            email,
            "John",
            "Doe",
            OrganizationRoleApi.Admin,
            "Password123!",
            TeamId: teamId,
            TeamRole: teamRole
        );

        client.WithOrganizationId(organizationId);

        return await client.PostAsJsonAsync("/api/users", request);
    }
}
