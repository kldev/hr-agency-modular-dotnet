namespace HrAgencySystem.ReportsService.Contracts;

/// <summary>
/// The one description of the reports service token, read by both sides - see
/// <c>FileServiceToken</c> for why these are constants rather than strings at call sites.
/// </summary>
public static class ReportsServiceToken
{
    public const string Issuer = "hr-api";
    public const string Audience = "reports-service";

    /// <summary>The organization an organization report is for. Required by those endpoints.</summary>
    public const string OrganizationClaim = "org";

    /// <summary>
    /// What the caller may see across tenants. Only <see cref="PlatformScope"/> exists: the platform
    /// owner's view. An organization token never carries it, and a platform token carries no
    /// organization - one token cannot open both doors.
    /// </summary>
    public const string ScopeClaim = "scope";

    public const string PlatformScope = "platform";

    /// <summary>The person behind the call, for the log line.</summary>
    public const string ActorClaim = "sub";

    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);
}
