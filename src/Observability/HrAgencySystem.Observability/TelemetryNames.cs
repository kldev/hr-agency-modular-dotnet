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

    /// <summary>
    /// Wolverine names its meter <c>Wolverine:&lt;service name&gt;</c>, so the exact name above - right
    /// for the activity source - would subscribe to nothing: handled, failed and dead-lettered
    /// messages, execution and effective time.
    /// </summary>
    public const string WolverineMeters = "Wolverine*";

    /// <summary>Sessions, appended events, and the async daemon's progress per projection.</summary>
    public const string Marten = "Marten";

    /// <summary>Connection pool and command metrics; the traces come from <c>AddNpgsql()</c>.</summary>
    public const string Npgsql = "Npgsql";

    /// <summary>
    /// The runtime's own meter (.NET 9+): CPU time, working set, GC, thread pool, exceptions - the
    /// <c>dotnet_*</c> series the dashboards read. Replaces the OpenTelemetry runtime package.
    /// </summary>
    public const string Runtime = "System.Runtime";
}
