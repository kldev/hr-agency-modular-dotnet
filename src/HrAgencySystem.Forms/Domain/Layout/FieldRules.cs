namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// Validation of one field, as data. One record with a slot per rule rather than a hierarchy of
/// rule types - the same trade <c>WorkRate</c> makes - so it serializes as a flat JSON object in
/// the event, in the frozen version, in OpenAPI and in the generated client alike. Null means "no
/// such rule".
/// <para>
/// Which slots mean anything depends on the <see cref="FieldType"/>; <see cref="FormLayoutPolicy"/>
/// refuses a rule the type cannot use, so a stored rule is always one that is enforced. The
/// catalogue is closed on purpose: no expressions, so the C# validator and its TypeScript mirror
/// cannot drift apart unnoticed - both are held to the same cases in
/// <c>tests/fixtures/forms-validation-cases.json</c>.
/// </para>
/// </summary>
public sealed record FieldRules(
    bool Required = false,
    int? MinLength = null,
    int? MaxLength = null,
    string? Pattern = null,
    decimal? Min = null,
    decimal? Max = null,
    int? Decimals = null,
    DateOnly? MinDate = null,
    DateOnly? MaxDate = null,
    int? MinSelected = null,
    int? MaxSelected = null,
    string? Message = null
)
{
    public static FieldRules None => new();
}
