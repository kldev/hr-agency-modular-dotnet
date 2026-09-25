namespace HrAgencySystem.Observability.Health;

public static class HealthTags
{
    /// <summary>
    /// A dependency the host cannot do its work without. <c>/health/ready</c> runs these;
    /// <c>/health/live</c> runs nothing, because a dead database is no reason to restart the process.
    /// </summary>
    public const string Ready = "ready";

    public static readonly string[] ReadyOnly = [Ready];
}
