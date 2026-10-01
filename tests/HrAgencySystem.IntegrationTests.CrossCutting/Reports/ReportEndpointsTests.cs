using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Infrastructure.Fakes;
using HrAgencySystem.ReportsService.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Reports;

/// <summary>
/// The API's half of reporting: who may ask, for which organization, over which months, and what a
/// dead reports service looks like to the caller. The service itself is faked here.
/// </summary>
[Collection(IntegrationCollection.Name)]
public sealed class ReportEndpointsTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private const string Recruitment = "/api/reports/recruitment";
    private const string Platform = "/api/owners/reports/platform";

    private readonly Guid _organizationId = Guid.NewGuid();

    private FakeReportsClient Reports =>
        (FakeReportsClient)Services.GetRequiredService<IReportsClient>();

    protected override Task BeforeEachAsync()
    {
        Reports.Reset();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RecruitmentReport_IsAskedForTheCallersOwnOrganization()
    {
        var client = OrganizationClient();

        var response = await client.GetAsync($"{Recruitment}?from=2026-01&to=2026-03");

        response.EnsureSuccessStatusCode();
        var report = await response.Content.ReadFromJsonAsync<RecruitmentReport>();
        Assert.Equal("2026-01", report!.From);
        Assert.Equal(3, report.Months.Count);

        var call = Assert.Single(Reports.Calls);
        Assert.Equal("recruitment", call.Report);
        Assert.Equal(_organizationId, call.OrganizationId);
        Assert.Equal("2026-03", call.Period.ToText);
    }

    [Fact]
    public async Task RecruitmentReport_WithoutAPeriod_CoversTheLastSixMonths()
    {
        var response = await OrganizationClient().GetAsync(Recruitment);

        response.EnsureSuccessStatusCode();
        Assert.Equal(ReportPeriod.DefaultMonths, Assert.Single(Reports.Calls).Period.MonthCount);
    }

    [Theory]
    [InlineData("from=2026-13", ReportPeriod.InvalidFromMessage)]
    [InlineData("from=2026-09&to=2026-01", ReportPeriod.ReversedMessage)]
    public async Task AMalformedPeriod_IsABadRequestThatNeverReachesTheService(
        string query,
        string message
    )
    {
        var response = await OrganizationClient().GetAsync($"{Recruitment}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(message, await response.Content.ReadAsStringAsync());
        Assert.Empty(Reports.Calls);
    }

    [Fact]
    public async Task RecruitmentExport_ReturnsTheSpreadsheet()
    {
        var response = await OrganizationClient()
            .GetAsync($"{Recruitment}/export?from=2026-01&to=2026-03");

        response.EnsureSuccessStatusCode();
        Assert.Equal(
            ReportFile.SpreadsheetContentType,
            response.Content.Headers.ContentType?.MediaType
        );
        Assert.Equal(
            "recruitment-2026-01-2026-03.xlsx",
            response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
        );
    }

    [Fact]
    public async Task ADeadReportsService_IsServiceUnavailable()
    {
        Reports.Unavailable = true;

        var response = await OrganizationClient().GetAsync(Recruitment);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task PlatformReport_IsForThePlatformOwner()
    {
        var response = await Env.CreateClient().AsOwner().GetAsync($"{Platform}?from=2026-04");

        response.EnsureSuccessStatusCode();
        var call = Assert.Single(Reports.Calls);
        Assert.Equal("platform", call.Report);
        Assert.Null(call.OrganizationId);
    }

    [Fact]
    public async Task PlatformReport_IsForbiddenToAnOrganizationUser()
    {
        var response = await OrganizationClient().GetAsync(Platform);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(Reports.Calls);
    }

    private HttpClient OrganizationClient()
    {
        var client = Env.CreateClient().AsOrganizationRoles();
        client.WithOrganizationId(_organizationId);
        return client;
    }
}
