using JetBrains.Annotations;

namespace HrAgencySystem.Api.Common.Config;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ApplicationConfig
{
    public const string Section = "Application";

    /// Base of the public job board (HrAgencySystem.Web), prefixed to every posting url.
    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string FeedUrl { get; init; } = "";

    /// Base of the links that leave the system in a mail - the reset password link, for one.
    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string PortalUrl { get; init; } = "";
}
