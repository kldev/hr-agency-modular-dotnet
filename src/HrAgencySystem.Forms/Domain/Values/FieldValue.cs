using System.Text.Json.Serialization;
using HrAgencySystem.Forms.Domain.Layout;

namespace HrAgencySystem.Forms.Domain.Values;

/// <summary>
/// An answer. One record with a slot per kind of value rather than a polymorphic union: the
/// codebase has no polymorphic JSON and this keeps it that way, while the stored JSONB stays typed -
/// a number is a number and a date is a date, which is what makes the answers searchable and, later,
/// reportable column by column.
/// <para>
/// The field's <see cref="FieldType"/> decides which slot is read (<see cref="SlotFor"/>); a value
/// in any other slot is refused by <see cref="Validation.FormAnswersValidator"/>.
/// </para>
/// </summary>
public sealed record FieldValue(
    string? Text = null,
    decimal? Number = null,
    DateOnly? Date = null,
    bool? Boolean = null,
    IReadOnlyList<string>? Values = null
)
{
    public static FieldValue OfText(string text) => new(Text: text);

    public static FieldValue OfDate(DateOnly date) => new(Date: date);

    /// <summary>Worked out, never stored - it would otherwise land in every event and in the API contract.</summary>
    [JsonIgnore]
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Text)
        && Number is null
        && Date is null
        && Boolean is null
        && (Values is null || Values.Count == 0);

    /// <summary>Equal by content - records compare a list by reference, which two identical selections are not.</summary>
    public bool SameAs(FieldValue other) =>
        Text == other.Text
        && Number == other.Number
        && Date == other.Date
        && Boolean == other.Boolean
        && (Values ?? []).SequenceEqual(other.Values ?? []);

    public static FieldValueSlot SlotFor(FieldType type) =>
        type switch
        {
            FieldType.Number => FieldValueSlot.Number,
            FieldType.Date => FieldValueSlot.Date,
            FieldType.Boolean => FieldValueSlot.Boolean,
            FieldType.MultiChoice => FieldValueSlot.Values,
            _ => FieldValueSlot.Text,
        };

    /// <summary>Whether anything sits outside the slot this type reads.</summary>
    public bool HasValueOutside(FieldValueSlot slot) =>
        (slot != FieldValueSlot.Text && Text is not null)
        || (slot != FieldValueSlot.Number && Number is not null)
        || (slot != FieldValueSlot.Date && Date is not null)
        || (slot != FieldValueSlot.Boolean && Boolean is not null)
        || (slot != FieldValueSlot.Values && Values is not null);
}

public enum FieldValueSlot
{
    Text,
    Number,
    Date,
    Boolean,
    Values,
}

/// <summary>An answer to one field of a form, named by the field's code.</summary>
public sealed record FieldAnswer(string FieldCode, FieldValue Value);
