using HrAgencySystem.FileService.Config;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.FileService.Application;

public sealed class UploadInspector(IOptions<FileServiceConfig> options) : IUploadInspector
{
    public const string EmptyFileMessage = "The file is empty.";
    public const string TooLargeMessage = "The file exceeds the maximum allowed size.";
    public const string UnsupportedTypeMessage = "This file type is not accepted.";
    public const string ExtensionMismatchMessage =
        "The file extension does not match its content type.";

    private readonly FileServiceConfig _config = options.Value;

    public string? Inspect(string contentType, string fileName, long size)
    {
        if (size <= 0)
            return EmptyFileMessage;

        if (size > _config.MaxSizeBytes)
            return TooLargeMessage;

        if (!_config.AllowedTypes.TryGetValue(Normalize(contentType), out var expected))
            return UnsupportedTypeMessage;

        var extension = Path.GetExtension(fileName);

        // An empty extension is fine - some clients send none. A contradicting one is not: it means
        // the two halves of the request disagree about what this file is, and we would be guessing.
        if (extension.Length > 0 && !Matches(extension, expected))
            return ExtensionMismatchMessage;

        return null;
    }

    public string ExtensionFor(string contentType) =>
        _config.AllowedTypes.TryGetValue(Normalize(contentType), out var extension)
            ? extension
            : throw new InvalidOperationException(UnsupportedTypeMessage);

    private static bool Matches(string extension, string expected)
    {
        if (extension.Equals(expected, StringComparison.OrdinalIgnoreCase))
            return true;

        // The one alias worth spelling out: both spellings are in the wild and neither is wrong.
        return expected is ".jpg" && extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string? contentType)
    {
        var value = (contentType ?? "").Trim();

        // "text/plain; charset=utf-8" is the same type as "text/plain".
        var separator = value.IndexOf(';');
        return separator < 0 ? value : value[..separator].Trim();
    }
}
