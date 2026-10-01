using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Fakes;

/// <summary>
/// Stands in for the reports service: remembers who asked for what, and can play dead. The service's
/// own queries are covered by its own integration tests; here it is the API's side that is under test.
/// </summary>
public sealed class FakeReportsClient : IReportsClient
{
    public sealed record Call(
        string Report,
        Guid? OrganizationId,
        Guid ActorId,
        ReportPeriod Period
    );

    private readonly List<Call> _calls = [];

    public IReadOnlyList<Call> Calls
    {
        get
        {
            lock (_calls)
                return [.. _calls];
        }
    }

    public bool Unavailable { get; set; }

    public void Reset()
    {
        lock (_calls)
            _calls.Clear();

        Unavailable = false;
    }

    public Task<RecruitmentReport> GetRecruitmentAsync(
        Guid organizationId,
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    )
    {
        Record("recruitment", organizationId, actorId, period);

        return Task.FromResult(
            new RecruitmentReport(
                period.FromText,
                period.ToText,
                new RecruitmentTotals(1, 2, 3, 4, 5, 6),
                new RecruitmentFunnel(2, 1, 1, 0, 0, 0, 1, 0, 0m),
                [
                    .. period
                        .Months()
                        .Select(m => new RecruitmentMonth(ReportPeriod.Write(m), 0, 0, 0, 0)),
                ],
                []
            )
        );
    }

    public Task<ReportFile> ExportRecruitmentAsync(
        Guid organizationId,
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    )
    {
        Record("recruitment-export", organizationId, actorId, period);

        return Task.FromResult(File($"recruitment-{period.FromText}-{period.ToText}.xlsx"));
    }

    public Task<PlatformReport> GetPlatformAsync(
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    )
    {
        Record("platform", null, actorId, period);

        return Task.FromResult(
            new PlatformReport(
                period.FromText,
                period.ToText,
                new PlatformTotals(0, 0, 0, 0, 0, 0, 0),
                []
            )
        );
    }

    public Task<ReportFile> ExportPlatformAsync(
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    )
    {
        Record("platform-export", null, actorId, period);

        return Task.FromResult(File($"platform-{period.FromText}-{period.ToText}.xlsx"));
    }

    private void Record(string report, Guid? organizationId, Guid actorId, ReportPeriod period)
    {
        if (Unavailable)
            throw new ReportsServiceException(ReportsServiceException.UnavailableMessage);

        lock (_calls)
            _calls.Add(new Call(report, organizationId, actorId, period));
    }

    private static ReportFile File(string name) =>
        new([0x50, 0x4B, 0x03, 0x04], name, ReportFile.SpreadsheetContentType);
}
