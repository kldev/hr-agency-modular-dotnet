using HrAgencySystem.ReportsService.Application.Platform;
using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.ReportsService.IntegrationTests;

[Collection(ReportsDatabaseCollection.Name)]
public sealed class PlatformReportQueryTests(ReportsDatabaseFixture database) : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 9, 23);

    private static readonly DateTimeOffset Founded = new(2025, 1, 5, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset August = new(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset September = new(2026, 9, 10, 9, 0, 0, TimeSpan.Zero);

    private ReportRows Rows => new(database.DataSource);

    private PlatformReportQuery Query => new(database.DataSource);

    public Task InitializeAsync() => database.CleanAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EveryOrganization_IsListed_MostRecentlyActiveFirst()
    {
        var busy = Guid.NewGuid();
        var quiet = Guid.NewGuid();

        await Rows.OrganizationAsync(busy, "HR Agency", Founded);
        await Rows.OrganizationAsync(quiet, "Tech Jobs", Founded);

        await Rows.JobPostAsync(busy, August, publishedAt: August);
        await Rows.ApplicationAsync(busy, August, "Offer", offerAt: September);
        await Rows.ApplicationAsync(busy, September, "Hired", hiredAt: September);
        await Rows.InterviewAsync(busy, August);
        await Rows.ProjectAsync(busy, August, "Active", wentLiveAt: September);
        await Rows.ProjectAsync(busy, Founded, "Completed", wentLiveAt: Founded);

        var report = await Query.RunAsync(
            ReportPeriod.Create("2026-08", "2026-09", Today),
            default
        );

        Assert.Equal(["HR Agency", "Tech Jobs"], report.Organizations.Select(o => o.Name));

        var agency = report.Organizations[0];
        Assert.Equal(busy, agency.OrganizationId);
        Assert.Equal(1, agency.JobPostsPublished);
        Assert.Equal(2, agency.Applications);
        Assert.Equal(1, agency.InterviewsScheduled);
        Assert.Equal(1, agency.Offers);
        Assert.Equal(1, agency.Hires);
        Assert.Equal(1, agency.ProjectsWentLive);
        Assert.Equal(1, agency.ProjectsActive);
        Assert.Equal(September, agency.LastActivityAt);

        // Nothing but the organization itself: last seen when it was created.
        var tech = report.Organizations[1];
        Assert.Equal(0, tech.Applications);
        Assert.Equal(Founded, tech.LastActivityAt);

        Assert.Equal(new PlatformTotals(2, 1, 1, 2, 1, 1, 1), report.Totals);
    }
}
