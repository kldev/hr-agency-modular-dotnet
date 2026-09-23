using HrAgencySystem.ReportsService.Application.Recruitment;
using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.ReportsService.IntegrationTests;

[Collection(ReportsDatabaseCollection.Name)]
public sealed class RecruitmentReportQueryTests(ReportsDatabaseFixture database) : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 9, 23);

    private static readonly DateTimeOffset July = new(2026, 7, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset August = new(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset September = new(2026, 9, 10, 9, 0, 0, TimeSpan.Zero);

    private readonly Guid _organization = Guid.NewGuid();
    private readonly Guid _otherOrganization = Guid.NewGuid();

    private ReportRows Rows => new(database.DataSource);

    private RecruitmentReportQuery Query => new(database.DataSource);

    public Task InitializeAsync() => database.CleanAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Totals_CountWhatHappenedInThePeriod()
    {
        await Rows.JobPostAsync(_organization, July, publishedAt: August);
        await Rows.JobPostAsync(_organization, July, publishedAt: July); // before the period
        await Rows.ApplicationAsync(_organization, August, "Offer", offerAt: September);
        await Rows.ApplicationAsync(_organization, September, "Hired", hiredAt: September);
        await Rows.InterviewAsync(_organization, August, completedAt: September);

        var report = await Query.RunAsync(_organization, Period("2026-08", "2026-09"), default);

        Assert.Equal(new RecruitmentTotals(1, 2, 1, 1, 1, 1), report.Totals);
    }

    [Fact]
    public async Task Funnel_FollowsTheApplicationsOfThePeriod_AndNeverWidens()
    {
        // Straight from screening to an offer: past the interview stage without an interview date.
        await Rows.ApplicationAsync(
            _organization,
            August,
            "Offer",
            screeningAt: August,
            offerAt: September
        );
        await Rows.ApplicationAsync(
            _organization,
            August,
            "Hired",
            screeningAt: August,
            interviewAt: August,
            offerAt: August,
            hiredAt: September
        );
        await Rows.ApplicationAsync(_organization, August, "Rejected", rejectedAt: August);
        await Rows.ApplicationAsync(_organization, August);
        // Received before the period: its September hire is activity, not part of this cohort.
        await Rows.ApplicationAsync(_organization, July, "Hired", hiredAt: September);

        var report = await Query.RunAsync(_organization, Period("2026-08", "2026-09"), default);

        var funnel = report.Funnel;
        Assert.Equal(4, funnel.Applied);
        Assert.Equal(2, funnel.Screening);
        Assert.Equal(2, funnel.Interview);
        Assert.Equal(2, funnel.Assessment);
        Assert.Equal(2, funnel.Offer);
        Assert.Equal(1, funnel.Hired);
        Assert.Equal(1, funnel.Rejected);
        Assert.Equal(0.25m, funnel.HireRate);
        Assert.Equal(2, report.Totals.Hires);
    }

    [Fact]
    public async Task Months_ListEveryMonthOfThePeriod_IncludingEmptyOnes()
    {
        await Rows.ApplicationAsync(_organization, July);
        await Rows.ApplicationAsync(_organization, September, "Hired", hiredAt: September);
        await Rows.InterviewAsync(_organization, September);

        var report = await Query.RunAsync(_organization, Period("2026-07", "2026-09"), default);

        Assert.Equal(
            [
                new RecruitmentMonth("2026-07", 1, 0, 0, 0),
                new RecruitmentMonth("2026-08", 0, 0, 0, 0),
                new RecruitmentMonth("2026-09", 1, 1, 0, 1),
            ],
            report.Months
        );
    }

    [Fact]
    public async Task TheFirstMomentOfTheNextMonth_IsOutsideThePeriod()
    {
        await Rows.ApplicationAsync(
            _organization,
            new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero)
        );
        await Rows.ApplicationAsync(
            _organization,
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero)
        );

        var report = await Query.RunAsync(_organization, Period("2026-09", "2026-09"), default);

        Assert.Equal(1, report.Totals.Applications);
    }

    [Fact]
    public async Task Sources_AreRankedByApplications()
    {
        await Rows.ApplicationAsync(_organization, August, source: "Referral");
        await Rows.ApplicationAsync(_organization, August, source: "JustJoinIt");
        await Rows.ApplicationAsync(_organization, September, source: "JustJoinIt");

        var report = await Query.RunAsync(_organization, Period("2026-08", "2026-09"), default);

        Assert.Equal(
            [new SourceCount("JustJoinIt", 2), new SourceCount("Referral", 1)],
            report.Sources
        );
    }

    [Fact]
    public async Task AnotherOrganizationsRows_AreNeverCounted()
    {
        await Rows.ApplicationAsync(_otherOrganization, August, "Hired", hiredAt: August);
        await Rows.JobPostAsync(_otherOrganization, August, publishedAt: August);

        var report = await Query.RunAsync(_organization, Period("2026-08", "2026-09"), default);

        Assert.Equal(new RecruitmentTotals(0, 0, 0, 0, 0, 0), report.Totals);
        Assert.Null(report.Funnel.HireRate);
        Assert.Empty(report.Sources);
    }

    private static ReportPeriod Period(string from, string to) =>
        ReportPeriod.Create(from, to, Today);
}
