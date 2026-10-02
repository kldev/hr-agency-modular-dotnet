namespace HrAgencySystem.Api.Infrastructure.ReportsClient;

public sealed class ReportsClientConfig
{
    public const string SectionName = "Reports";

    public string BaseUrl { get; init; } = "http://localhost:5200";

    /// <summary>
    /// Shared with the reports service and nothing else - not the user token key, not the file
    /// service's. One key per audience, so no token opens a door it was not minted for.
    /// </summary>
    public string Secret { get; init; } = "";

    public int TimeoutSeconds { get; init; } = 30;
}
