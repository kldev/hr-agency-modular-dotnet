namespace HrAgencySystem.ReportsService.Config;

public sealed class ReportsServiceConfig
{
    public const string SectionName = "Reports";

    /// <summary>
    /// The key service tokens are signed with. Must differ from the API's user token key and from
    /// the file service's - one key for two audiences would make one token open both.
    /// </summary>
    public string Secret { get; init; } = "";
}
