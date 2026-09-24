using System.Net.Http.Headers;
using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.Api.Infrastructure.ReportsClient;

public sealed class HttpReportsClient(
    HttpClient http,
    ReportsTokenFactory tokens,
    ILogger<HttpReportsClient> logger
) : IReportsClient
{
    public Task<RecruitmentReport> GetRecruitmentAsync(
        Guid organizationId,
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    ) =>
        GetJsonAsync<RecruitmentReport>(
            ReportsServiceRoutes.Recruitment,
            period,
            tokens.ForOrganization(organizationId, actorId),
            ct
        );

    public Task<ReportFile> ExportRecruitmentAsync(
        Guid organizationId,
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    ) =>
        GetFileAsync(
            ReportsServiceRoutes.RecruitmentExport,
            period,
            tokens.ForOrganization(organizationId, actorId),
            ct
        );

    public Task<PlatformReport> GetPlatformAsync(
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    ) =>
        GetJsonAsync<PlatformReport>(
            ReportsServiceRoutes.Platform,
            period,
            tokens.ForPlatform(actorId),
            ct
        );

    public Task<ReportFile> ExportPlatformAsync(
        Guid actorId,
        ReportPeriod period,
        CancellationToken ct
    ) => GetFileAsync(ReportsServiceRoutes.PlatformExport, period, tokens.ForPlatform(actorId), ct);

    private async Task<T> GetJsonAsync<T>(
        string route,
        ReportPeriod period,
        string token,
        CancellationToken ct
    )
    {
        using var response = await Send(route, period, token, ct);

        return await response.Content.ReadFromJsonAsync<T>(ct)
            ?? throw new ReportsServiceException(ReportsServiceException.UnavailableMessage);
    }

    private async Task<ReportFile> GetFileAsync(
        string route,
        ReportPeriod period,
        string token,
        CancellationToken ct
    )
    {
        using var response = await Send(route, period, token, ct);

        var headers = response.Content.Headers;

        return new ReportFile(
            await response.Content.ReadAsByteArrayAsync(ct),
            headers.ContentDisposition?.FileNameStar
                ?? headers.ContentDisposition?.FileName?.Trim('"')
                ?? "report.xlsx",
            headers.ContentType?.ToString() ?? ReportFile.SpreadsheetContentType
        );
    }

    /// <summary>
    /// Any failure is the service's, not the caller's: the period was validated before the call,
    /// so a 4xx here means the two sides disagree about the contract - a fault, reported as 503.
    /// </summary>
    private async Task<HttpResponseMessage> Send(
        string route,
        ReportPeriod period,
        string token,
        CancellationToken ct
    )
    {
        var uri =
            $"{route}?{ReportsServiceRoutes.FromQuery}={period.FromText}"
            + $"&{ReportsServiceRoutes.ToQuery}={period.ToText}";

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;

        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "The reports service could not be reached at {Uri}.", uri);
            throw new ReportsServiceException(ReportsServiceException.UnavailableMessage, ex);
        }

        if (response.IsSuccessStatusCode)
            return response;

        logger.LogError(
            "The reports service answered {Status} for {Uri}.",
            (int)response.StatusCode,
            uri
        );
        response.Dispose();

        throw new ReportsServiceException(
            $"{ReportsServiceException.UnavailableMessage} ({(int)response.StatusCode})"
        );
    }
}
