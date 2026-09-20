namespace HrAgencySystem.FileService.Application;

/// <summary>
/// Everything that has to be true about an upload before a single byte reaches the bucket. One
/// implementation today; virus scanning would be a second one behind the same call, which is why it
/// is an interface and not four ifs in the handler.
/// </summary>
public interface IUploadInspector
{
    /// <returns>The reason to refuse, or <c>null</c> when the upload may proceed.</returns>
    string? Inspect(string contentType, string fileName, long size);

    /// <summary>The extension this content type is stored under. Never the uploader's.</summary>
    string ExtensionFor(string contentType);
}
