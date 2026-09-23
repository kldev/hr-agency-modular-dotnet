namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// What kind of answer a field takes. The type decides which slot of <see cref="Values.FieldValue"/>
/// holds the answer, which rules of <see cref="FieldRules"/> mean anything and which control the
/// front end draws - nothing else in the module branches on it.
/// <para>
/// Append only: Marten stores the ordinal, so reordering these would silently turn every stored
/// date into a number. <c>DateTime</c>, <c>Time</c>, <c>Money</c>, <c>File</c> and
/// <c>Signature</c> are the next ones in line (plan 028, later stages).
/// </para>
/// </summary>
public enum FieldType
{
    Text,
    TextArea,
    Number,
    Date,
    Boolean,
    SingleChoice,
    MultiChoice,
    Email,
    Phone,
    Country,
}

public static class FieldTypes
{
    extension(FieldType type)
    {
        /// <summary>Types whose answer is a string in <see cref="Values.FieldValue.Text"/>.</summary>
        public bool IsTextual =>
            type
                is FieldType.Text
                    or FieldType.TextArea
                    or FieldType.Email
                    or FieldType.Phone
                    or FieldType.Country
                    or FieldType.SingleChoice;

        public bool HasOptions => type is FieldType.SingleChoice or FieldType.MultiChoice;

        /// <summary>Length and pattern apply to what somebody types, not to a picked value.</summary>
        public bool TakesTypedText =>
            type is FieldType.Text or FieldType.TextArea or FieldType.Email or FieldType.Phone;
    }
}
