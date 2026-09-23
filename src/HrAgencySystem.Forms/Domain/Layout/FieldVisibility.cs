using HrAgencySystem.Forms.Domain.Values;

namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// "Show this field when that one…" - <b>reserved, not evaluated anywhere yet</b>. The slot exists
/// so that conditional fields (plan 028, later stages) are an addition rather than a change of
/// shape; until then <see cref="FormLayoutPolicy"/> refuses any value here, so nobody can believe it
/// works.
/// </summary>
public sealed record FieldVisibility(string FieldCode, VisibilityOperator Operator, FieldValue? Value);

public enum VisibilityOperator
{
    Equals,
    NotEquals,
    IsFilled,
}
