namespace HrAgencySystem.ReportsService.Contracts;

/// <summary>The service's routes, shared so the client and the endpoints cannot drift apart.</summary>
public static class ReportsServiceRoutes
{
    public const string Health = "/healthz";

    public const string Recruitment = "/reports/recruitment";
    public const string RecruitmentExport = "/reports/recruitment/export";
    public const string Platform = "/reports/platform";
    public const string PlatformExport = "/reports/platform/export";

    public const string FromQuery = "from";
    public const string ToQuery = "to";
}
