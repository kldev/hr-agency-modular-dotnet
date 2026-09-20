namespace HrAgencySystem.FileService.Config;

public sealed class FileServiceConfig
{
    public const string SectionName = "FileService";

    /// <summary>
    /// Signing key for the service token. Must differ from the API's user token secret - sharing one
    /// would turn every user token into a service token.
    /// </summary>
    public string Secret { get; set; } = "";

    public long MaxSizeBytes { get; set; } = 25 * 1024 * 1024;

    /// <summary>
    /// Content type to file extension. The allowlist lives here rather than in code so a deployment
    /// can narrow it, but it ships populated: a missing configuration section must not silently mean
    /// "accept anything".
    /// </summary>
    public Dictionary<string, string> AllowedTypes { get; set; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = ".pdf",
            ["image/png"] = ".png",
            ["image/jpeg"] = ".jpg",
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = ".docx",
            ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = ".xlsx",
            ["application/vnd.oasis.opendocument.text"] = ".odt",
            ["application/vnd.oasis.opendocument.spreadsheet"] = ".ods",
            ["text/plain"] = ".txt",
            ["message/rfc822"] = ".eml",
        };
}
