namespace HrAgencySystem.Identity.Infrastructure.Configuration;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class IdentityConfig
{
    public const string Section = "Identity";

    /// How long a password reset link stays usable. The saga schedules its own timeout from this,
    /// so changing it here changes the window - there is no second place holding an expiry.
    public int PasswordResetExpiresInMinutes { get; set; } = 15;
}
