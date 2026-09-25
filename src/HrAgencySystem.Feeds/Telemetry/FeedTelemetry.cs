using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HrAgencySystem.Feeds.Telemetry;

/// <summary>
/// Feed generation runs on a timer, so no request opens a trace for it: without the span here every
/// SQL query and S3 call of one generation would be a root span of its own, unrelated to the rest.
/// </summary>
public sealed class FeedTelemetry
{
    public const string Name = "HrAgencySystem.Feeds";

    public const string Completed = "completed";
    public const string Failed = "failed";

    public static readonly ActivitySource Source = new(Name);

    private readonly Counter<long> _generations;
    private readonly Histogram<double> _duration;
    private readonly Histogram<long> _size;

    public FeedTelemetry(IMeterFactory meters)
    {
        var meter = meters.Create(Name);

        _generations = meter.CreateCounter<long>(
            "hr.feeds.generations",
            unit: "{feed}",
            description: "Feed generation tasks by outcome (completed, failed)."
        );
        _duration = meter.CreateHistogram<double>(
            "hr.feeds.generation.duration",
            unit: "s",
            description: "Reading the posts, serializing both formats and storing them."
        );
        _size = meter.CreateHistogram<long>(
            "hr.feeds.size",
            unit: "By",
            description: "Size of a generated feed file, by format - a feed that keeps growing is a post that never closes."
        );
    }

    public void RecordGeneration(string outcome, TimeSpan elapsed)
    {
        _generations.Add(1, new KeyValuePair<string, object?>("outcome", outcome));
        _duration.Record(elapsed.TotalSeconds, new KeyValuePair<string, object?>("outcome", outcome));
    }

    public void RecordSize(string format, long bytes) =>
        _size.Record(bytes, new KeyValuePair<string, object?>("format", format));
}
