using System.Diagnostics.Metrics;
using HrAgencySystem.FileService.Application;

namespace HrAgencySystem.FileService.Infrastructure.Telemetry;

/// <summary>
/// What people try to upload and how much of it the inspector turns away. A rise in
/// <c>extension_mismatch</c> is somebody renaming files; a rise in <c>too_large</c> is a limit
/// that no longer fits the documents the agency actually receives.
/// </summary>
public sealed class FileMetrics
{
    public const string MeterName = "HrAgencySystem.Files";

    public const string Stored = "stored";
    public const string Rejected = "rejected";

    private readonly Counter<long> _uploads;
    private readonly Histogram<long> _size;

    public FileMetrics(IMeterFactory meters)
    {
        var meter = meters.Create(MeterName);

        _uploads = meter.CreateCounter<long>(
            "hr.files.uploads",
            unit: "{file}",
            description: "Uploads by outcome (stored, rejected) and rejection reason."
        );
        _size = meter.CreateHistogram<long>(
            "hr.files.upload.size",
            unit: "By",
            description: "Size of the files that were stored, by content type."
        );
    }

    public void RecordStored(string contentType, long size)
    {
        _uploads.Add(1, new KeyValuePair<string, object?>("outcome", Stored));
        _size.Record(size, new KeyValuePair<string, object?>("content_type", contentType));
    }

    /// <summary>
    /// The reason becomes a short code, never the message itself, so the tag stays a closed set
    /// even if a message is reworded.
    /// </summary>
    public void RecordRejected(string rejection) =>
        _uploads.Add(
            1,
            new KeyValuePair<string, object?>("outcome", Rejected),
            new KeyValuePair<string, object?>("reason", ReasonOf(rejection))
        );

    public static string ReasonOf(string rejection) =>
        rejection switch
        {
            UploadInspector.EmptyFileMessage => "empty",
            UploadInspector.TooLargeMessage => "too_large",
            UploadInspector.UnsupportedTypeMessage => "unsupported_type",
            UploadInspector.ExtensionMismatchMessage => "extension_mismatch",
            _ => "other",
        };
}
