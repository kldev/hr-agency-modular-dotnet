namespace HrAgencySystem.Observability;

/// <summary>
/// The activity sources and meters a host listens to. A source nobody subscribes to costs nothing
/// and emits nothing, so a name missing here is silently absent from every dashboard.
/// </summary>
public static class TelemetryNames
{
    /// <summary>Our own sources and meters: <c>HrAgencySystem.&lt;Module&gt;</c>.</summary>
    public const string Application = "HrAgencySystem.*";

    /// <summary>Handlers, outbox, RabbitMQ send and receive - and the trace context between them.</summary>
    public const string Wolverine = "Wolverine";

    /// <summary>Sessions, appended events, and the async daemon's progress per projection.</summary>
    public const string Marten = "Marten";

    /// <summary>Connection pool and command metrics; the traces come from <c>AddNpgsql()</c>.</summary>
    public const string Npgsql = "Npgsql";
}
