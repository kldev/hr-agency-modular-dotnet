using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Identity.Application.Policy;

/// <summary>
/// What counts as a profile picture. The rule lives here rather than in the file service, whose size
/// cap and type allow-list are shared by every owner of every file - narrowing those to suit avatars
/// would narrow them for project documents too.
/// </summary>
public static class AvatarUploadPolicy
{
    public const long MaxSizeBytes = 512 * 1024;

    public const string EmptyFileMessage = "The picture is empty.";

    public const string TooLargeMessage = "The picture must be 512 KB or smaller.";

    public const string UnsupportedTypeMessage = "The picture must be a PNG or JPEG image.";

    private static readonly HashSet<string> AllowedContentTypes = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "image/png",
        "image/jpeg",
    };

    public static void Validate(string? contentType, long size)
    {
        var errors = new List<string>();

        if (size <= 0)
            errors.Add(EmptyFileMessage);
        else if (size > MaxSizeBytes)
            errors.Add(TooLargeMessage);

        if (!AllowedContentTypes.Contains(Normalize(contentType)))
            errors.Add(UnsupportedTypeMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    // "image/png; charset=binary" is the same type as "image/png".
    private static string Normalize(string? contentType)
    {
        var value = (contentType ?? "").Trim();

        var separator = value.IndexOf(';');

        return separator < 0 ? value : value[..separator].Trim();
    }
}
