using JetBrains.Annotations;

namespace HrAgencySystem.Api.Infrastructure.FileServiceClient;

public sealed class FileServiceClientConfig
{
    public const string SectionName = "FileService";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string BaseUrl { get; init; } = "http://localhost:5100";

    /// <summary>
    /// Shared with the file service and with nothing else. Deliberately not the JWT secret users are
    /// issued tokens with: one key for both would make every user token a service token.
    /// </summary>
    public string Secret { get; init; } = "";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public int TimeoutSeconds { get; init; } = 60;
}
