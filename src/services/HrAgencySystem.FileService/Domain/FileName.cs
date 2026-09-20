namespace HrAgencySystem.FileService.Domain;

/// <summary>
/// The original file name, kept only to show it back and to name the download. It never reaches the
/// storage key, so this is about readability rather than safety - but a name carrying a path or a
/// control character would be a nuisance in every UI that renders it, so it is cleaned here once.
/// </summary>
public static class FileName
{
    public const int MaxLength = 255;
    public const string Fallback = "file";

    public static string Sanitize(string? input)
    {
        var candidate = (input ?? "").Replace('\\', '/');

        // Path.GetFileName on the last segment drops "../.." and anything else that looks like a
        // path, whichever separator the uploading platform used.
        candidate = Path.GetFileName(candidate);

        candidate = new string([.. candidate.Where(c => !char.IsControl(c))]).Trim();

        if (candidate.Length == 0 || candidate.All(c => c == '.'))
            return Fallback;

        return candidate.Length <= MaxLength ? candidate : Truncate(candidate);
    }

    private static string Truncate(string candidate)
    {
        var extension = Path.GetExtension(candidate);

        if (extension.Length is 0 or > 16)
            return candidate[..MaxLength];

        return candidate[..(MaxLength - extension.Length)] + extension;
    }
}
