namespace HrAgencySystem.Api.Common.Config;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ApplicationConfig
{
    public const string Section = "Application";

    /// Base of the public job board (HrAgencySystem.Web), prefixed to every posting url.
    public string FeedUrl { get; set; } = "";

    /// Base of the links that leave the system in a mail - the reset password link, for one.
    public string PortalUrl { get; set; } = "";
}
