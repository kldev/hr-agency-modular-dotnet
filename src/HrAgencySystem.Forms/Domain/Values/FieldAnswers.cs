using HrAgencySystem.Forms.Domain.Layout;

namespace HrAgencySystem.Forms.Domain.Values;

/// <summary>
/// Puts answers into the one shape that is stored: text trimmed, a country upper-cased, a selection
/// without repeats, and an empty answer dropped rather than kept as an empty string - "not answered"
/// has one representation, which is what a search for "who left it blank" relies on.
/// <para>
/// An answer to a code the layout does not know is passed through untouched, so the validator can
/// name it instead of it silently disappearing.
/// </para>
/// </summary>
public static class FieldAnswers
{
    public static IReadOnlyList<FieldAnswer> Normalize(
        IReadOnlyList<FormPage> pages,
        IReadOnlyList<FieldAnswer>? answers
    )
    {
        var normalized = new List<FieldAnswer>();

        foreach (var answer in answers ?? [])
        {
            var field = pages.FieldByCode(answer.FieldCode);
            var value = field is null ? answer.Value : Normalize(field.Type, answer.Value);

            if (value is not null && !value.IsEmpty)
                normalized.Add(answer with { Value = value });
        }

        return normalized;
    }

    private static FieldValue? Normalize(FieldType type, FieldValue? value)
    {
        if (value is null)
            return null;

        var text = string.IsNullOrWhiteSpace(value.Text) ? null : value.Text.Trim();

        if (type == FieldType.Country)
            text = text?.ToUpperInvariant();

        IReadOnlyList<string>? values = value.Values is null
            ? null
            : [.. value.Values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct()];

        return value with { Text = text, Values = values is { Count: 0 } ? null : values };
    }
}
