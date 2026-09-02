using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public static class TestAuthenticationExtensions
{
    public static void WithOrganizationId(this HttpClient client, Guid organizationId)
    {
        client.DefaultRequestHeaders.Remove("X-Test-OrganizationId");
        client.DefaultRequestHeaders.Add("X-Test-OrganizationId", organizationId.ToString());
    }
    public static HttpClient AsOwner(this HttpClient client)
    {
        client.SetTestRoles(nameof(PlatformRole.Owner));
        return client;
    }

    public static HttpClient AsOrganizationRoles(this HttpClient client)
    {
        client.SetTestRoles(nameof(OrganizationRole.Admin));
        return client;
    }

    public static void SetTestRoles(
        this HttpClient client,
        params string[] roles)
    {
        client.DefaultRequestHeaders.Remove("X-Test-Roles");
        client.DefaultRequestHeaders.Add(
            "X-Test-Roles",
            string.Join(",", roles));
    }
}