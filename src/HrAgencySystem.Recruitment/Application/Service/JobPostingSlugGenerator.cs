using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace HrAgencySystem.Recruitment.Application.Service;

public sealed class JobPostingSlugGenerator
{
    public string Generate(
        string? companyName,
        string? title,
        string? location,
        Guid postingId)
    {
        var baseSlug = string.Join(
            "-",
            new[] { companyName, title, location }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(Slugify));

        var suffix = postingId
            .ToString("N")
            .Substring(0, 4);

        return $"{baseSlug}-{suffix}";
    }

    private static string Slugify(string value)
    {
        var normalized = value
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(character);
        }

        return Regex.Replace(
                builder.ToString(),
                "[^a-z0-9]+",
                "-")
            .Trim('-');
    }
}