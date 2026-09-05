namespace HrAgencySystem.Files.Config;

public class S3Config
{
    public const string SectionName = "RustFs";

    public string Endpoint { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
}