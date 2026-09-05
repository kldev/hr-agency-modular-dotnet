using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace HrAgencySystem.Recruitment.Application.Service;

internal static partial class JobPostingSlugGenerator
{
    public static string Generate(
        string? companyName,
        string? title,
        string? location,
        Guid postingId)
    {
        var baseSlug = string.Join(
            "-",
            new[] { companyName, title, location }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(Slugify!));

        var suffix = postingId
            .ToString("N")[..4];

        return $"{baseSlug}-{suffix}";
    }

    private static string Slugify(string value)
    {
        var normalized = value
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in from character in normalized let category = CharUnicodeInfo.GetUnicodeCategory(character) where category != UnicodeCategory.NonSpacingMark select character)
        {
            builder.Append(character);
        }

        return MyRegex().Replace(builder.ToString(), "-")
            .Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex MyRegex();
}