using System.Net;
using HrAgencySystem.Api;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Hosting;

/// <summary>
/// The host under test runs as "Testing", the way production runs as anything but Development:
/// a factory that silently fell back to Development would test seeding endpoints nobody deploys.
/// </summary>
[Collection(IntegrationCollection.Name)]
public sealed class TestingEnvironmentTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    [Theory]
    [InlineData(ApiEndpoints.Development.Seed)]
    [InlineData(ApiEndpoints.Development.SeedSales)]
    public async Task Development_endpoints_are_not_mapped(string route)
    {
        // Act
        var response = await Client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
