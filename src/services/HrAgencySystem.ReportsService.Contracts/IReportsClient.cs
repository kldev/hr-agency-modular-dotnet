namespace HrAgencySystem.ReportsService.Contracts;

/// <summary>
/// How the API asks for a report. Who is asking travels in the signed token the implementation
/// mints, never as a parameter the service would have to trust.
/// </summary>
public interface IReportsClient
{
    Task<RecruitmentReport> GetRecruitmentAsync(
        Guid organizationId,
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    );

    Task<ReportFile> ExportRecruitmentAsync(
        Guid organizationId,
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    );

    Task<PlatformReport> GetPlatformAsync(Guid actorId, ReportPeriod period, CancellationToken ct);

    Task<ReportFile> ExportPlatformAsync(Guid actorId, ReportPeriod period, CancellationToken ct);
}

/// <summary>A generated file: small enough to hold in memory, so bytes rather than a stream.</summary>
public sealed record ReportFile(byte[] Content, string FileName, string ContentType)
{
    public const string SpreadsheetContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
