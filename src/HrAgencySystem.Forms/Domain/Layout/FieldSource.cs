namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// Where a field's definition comes from - the question that replaced "mnemonic" (plan 028 §2).
/// <para>
/// <see cref="System"/>: the organization's catalogue. The value is a fact about the person, one
/// value shown in every form that asks for it. <see cref="Form"/>: this form alone. The value is an
/// answer in this document - the same looking "contract end date" in another form is a different
/// answer, because it is about a different contract. Both have a code, and the code is what reports
/// ask by.
/// </para>
/// </summary>
public enum FieldSource
{
    System,
    Form,
}
