using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Organization;

public sealed class OrganizationTestClient(
    HttpClient client,
    ITestOutputHelper output)
{
    public async Task<OrganizationCreated> CreateAsync(
        string name = "HR Agency",
        string slug = "hr-agency")
    {
        var request = new CreateOrganization(
            name,
            slug,
            Guid.NewGuid());

        var response = await client.PostAsJsonAsync(
            "/api/organization",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<OrganizationCreated>(
            output);

        Assert.NotNull(result);

        return result;
    }
}