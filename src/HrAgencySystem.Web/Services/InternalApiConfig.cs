using JetBrains.Annotations;

namespace HrAgencySystem.Web.Services;

/// <summary>
/// Where the API is and the key this host presents to it. The key is issued by the platform owner
/// (<c>POST /api/owners/api-keys</c>), is shown once, and reads <c>sk_…</c>; it belongs in user
/// secrets or the environment, never in a committed appsettings file.
/// </summary>
public sealed class InternalApiConfig
{
    public const string Section = "InternalApi";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string BaseUrl { get; init; } = "";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string ApiKey { get; init; } = "";

    /// <summary>How long one call may take before the page gives up and says so.</summary>
    public int TimeoutSeconds { get; init; } = 10;
}
