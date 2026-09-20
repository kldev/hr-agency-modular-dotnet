namespace HrAgencySystem.Files.Model;

public sealed record ObjectMetadata(
    string ContentType,
    long Size,
    DateTimeOffset LastModified,
    string ETag
);
