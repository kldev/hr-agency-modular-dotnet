namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

public sealed class FileServiceClientConfig
{
    public const string SectionName = "FileService";

    public string BaseUrl { get; init; } = "http://localhost:5100";

    /// <summary>
    /// Shared with the file service and with nothing else. Deliberately not the JWT secret users are
    /// issued tokens with: one key for both would make every user token a service token.
    /// </summary>
    public string Secret { get; init; } = "";

    public int TimeoutSeconds { get; init; } = 60;
}
