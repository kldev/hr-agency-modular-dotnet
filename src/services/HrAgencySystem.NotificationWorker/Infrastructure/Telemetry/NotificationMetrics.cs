using System.Diagnostics.Metrics;

namespace HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

/// <summary>
/// Whether mail leaves, per template. Wolverine already counts handled and failed messages; what it
/// cannot see is the idempotency guard turning a redelivery into a no-op, and that is exactly the
/// number that tells a duplicate storm from real traffic.
/// </summary>
public sealed class NotificationMetrics
{
    public const string MeterName = "HrAgencySystem.Notifications";

    public const string Sent = "sent";
    public const string Duplicate = "duplicate";
    public const string Failed = "failed";

    private readonly Counter<long> _emails;
    private readonly Histogram<double> _duration;

    public NotificationMetrics(IMeterFactory meters)
    {
        var meter = meters.Create(MeterName);

        _emails = meter.CreateCounter<long>(
            "hr.notifications.emails",
            unit: "{email}",
            description: "Mail deliveries by template and outcome (sent, duplicate, failed)."
        );
        _duration = meter.CreateHistogram<double>(
            "hr.notifications.email.duration",
            unit: "s",
            description: "Rendering plus the SMTP conversation, for deliveries that were attempted."
        );
    }

    public void Record(string template, string outcome) =>
        _emails.Add(1, Template(template), new("outcome", outcome));

    public void RecordDuration(string template, TimeSpan elapsed) =>
        _duration.Record(elapsed.TotalSeconds, Template(template));

    private static KeyValuePair<string, object?> Template(string template) => new("template", template);
}
