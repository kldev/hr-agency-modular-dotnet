namespace HrAgencySystem.Web.Services;

/// <summary>
/// Where the API is and the key this host presents to it. The key is issued by the platform owner
/// (<c>POST /api/owners/api-keys</c>), is shown once, and reads <c>sk_…</c>; it belongs in user
/// secrets or the environment, never in a committed appsettings file.
/// </summary>
public sealed class InternalApiConfig
{
    public const string Section = "InternalApi";

    public string BaseUrl { get; set; } = "";

    public string ApiKey { get; set; } = "";

    /// <summary>How long one call may take before the page gives up and says so.</summary>
    public int TimeoutSeconds { get; set; } = 10;
}
