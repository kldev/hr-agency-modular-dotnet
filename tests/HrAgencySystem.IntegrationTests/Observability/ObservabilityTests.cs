using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Observability.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Observability;

[Collection(IntegrationCollection.Name)]
public class ObservabilityTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    [Fact]
    public async Task Liveness_answers_without_a_token_and_without_asking_dependencies()
    {
        // The broker is stubbed out in these tests, so a liveness probe that asked it would fail.
        using var anonymous = Env.CreateClient();

        var response = await anonymous.GetAsync(HealthEndpointsExtensions.Live);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Problem_details_carry_the_w3c_trace_id()
    {
        Client.WithOrganizationId(Guid.NewGuid());

        var response = await Client.GetAsync($"/api/projects/{Guid.NewGuid()}");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        var traceId = Assert.Contains("traceId", problem.Extensions)?.ToString();
        // 32 hex characters: the bare trace id a trace viewer searches by, not Activity.Id.
        Assert.Matches("^[0-9a-f]{32}$", traceId);
    }
}
