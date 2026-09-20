namespace HrAgencySystem.FileService.Contracts;

/// <summary>
/// Everything a caller may know about a stored file. Deliberately without the storage key: a domain
/// that can name an object in the bucket is a domain that can be talked into reading someone else's.
/// </summary>
public sealed record FileDescriptor(
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    string Sha256,
    DateTimeOffset UploadedAt
);

/// <summary>
/// The bytes plus the little that is needed to serve them. The caller owns <see cref="Content"/> and
/// is responsible for disposing it.
/// </summary>
public sealed record FileContent(Stream Content, string FileName, string ContentType, long Size);
