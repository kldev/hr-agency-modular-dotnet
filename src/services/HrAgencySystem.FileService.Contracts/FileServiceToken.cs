namespace HrAgencySystem.FileService.Contracts;

/// <summary>
/// The one description of the service token, read by both sides. An issuer, an audience and a claim
/// name are part of the wire contract: spelling them at the call site is how two processes end up
/// disagreeing about what a valid token looks like.
/// </summary>
public static class FileServiceToken
{
    public const string Issuer = "hr-api";
    public const string Audience = "file-service";

    /// <summary>The organization the call is made on behalf of. A token without it is rejected.</summary>
    public const string OrganizationClaim = "org";

    /// <summary>The person behind the call, kept for the upload record.</summary>
    public const string ActorClaim = "sub";

    /// <summary>
    /// Long enough to survive a slow upload handshake, short enough that a captured token is worth
    /// nothing. The token authorises a call, not a session.
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);
}
