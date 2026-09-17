namespace HrAgencySystem.Feeds.Config;

public sealed class FeedsConfig
{
    public const string Section = "Application";

    /// <summary>
    /// Public base address the feed links point at. Must match the value used by the API host,
    /// because it is prepended to every posting slug in the generated files.
    /// </summary>
    public string FeedUrl { get; set; } = "";
}
