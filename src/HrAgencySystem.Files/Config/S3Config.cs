using JetBrains.Annotations;

namespace HrAgencySystem.Files.Config;

public class S3Config
{
    public const string SectionName = "RustFs";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string Endpoint { get; init; } = "";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string AccessKey { get; init; } = "";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string SecretKey { get; init; } = "";

    /// <summary>
    /// RustFS and MinIO style endpoints serve buckets as a path segment; real AWS S3 serves them as
    /// a subdomain. Hard-coding either one means the same library cannot talk to the other.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public bool ForcePathStyle { get; init; } = true;

    /// <summary>
    /// Irrelevant for RustFS, which ignores it, and load bearing for AWS S3, which does not.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string Region { get; init; } = "us-east-1";
}
