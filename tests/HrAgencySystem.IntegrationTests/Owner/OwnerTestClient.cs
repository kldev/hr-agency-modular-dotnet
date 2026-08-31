using System.Net.Http.Json;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Owner;

public sealed class OwnerTestClient(
    HttpClient client,
    ITestOutputHelper output)
{
    public async Task<OwnerProjection> CreateAsync(
        string email = "owner@test.com",
        string password = "Password123!")
    {
        var request = new CreatePlatformOwner(
            email,
            password);

        var response = await client.PostAsJsonAsync(
            "/api/owners",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<OwnerProjection>(
            output);

        Assert.NotNull(result);

        return result;
    }
}