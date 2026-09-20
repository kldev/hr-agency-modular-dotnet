namespace HrAgencySystem.Files.Config;

public class S3Config
{
    public const string SectionName = "RustFs";

    public string Endpoint { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";

    /// <summary>
    /// RustFS and MinIO style endpoints serve buckets as a path segment; real AWS S3 serves them as
    /// a subdomain. Hard-coding either one means the same library cannot talk to the other.
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>
    /// Irrelevant for RustFS, which ignores it, and load bearing for AWS S3, which does not.
    /// </summary>
    public string Region { get; set; } = "us-east-1";
}
