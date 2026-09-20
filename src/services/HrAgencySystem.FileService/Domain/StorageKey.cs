namespace HrAgencySystem.FileService.Domain;

/// <summary>
/// Where an object lives in the bucket. Built here and only here: there is no endpoint that accepts
/// a key, so asking for someone else's object is not something a caller can express.
/// </summary>
public static class StorageKey
{
    public const string InvalidOwnerKindMessage =
        "Owner kind must be 1-32 lowercase letters, digits or dashes.";

    public static string Create(
        Guid organizationId,
        string ownerKind,
        Guid ownerId,
        Guid fileId,
        string extension
    )
    {
        return $"{organizationId:N}/{NormalizeOwnerKind(ownerKind)}/{ownerId:N}/{fileId:N}{extension}";
    }

    /// <summary>
    /// The owner kind reaches us over the wire and ends up in a path, so it is constrained rather
    /// than escaped - a value that cannot contain a slash cannot climb out of its prefix.
    /// </summary>
    public static string NormalizeOwnerKind(string? ownerKind)
    {
        var normalized = (ownerKind ?? "").Trim().ToLowerInvariant();

        if (normalized.Length is 0 or > 32)
            throw new ArgumentException(InvalidOwnerKindMessage, nameof(ownerKind));

        if (!normalized.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '-'))
            throw new ArgumentException(InvalidOwnerKindMessage, nameof(ownerKind));

        return normalized;
    }
}
