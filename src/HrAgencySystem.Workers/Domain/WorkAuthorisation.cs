namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// One permission a person holds to be in a country and to work there, with the period it covers.
/// <para>
/// This belongs to the person, not to any assignment: a residence card does not stop being valid
/// because a project ended, and the next project does not need a new one. That is exactly what
/// separates it from an A1, which is issued for one posting and dies with it.
/// </para>
/// </summary>
public sealed record WorkAuthorisation(
    Guid AuthorisationId,
    WorkAuthorisationKind Kind,
    string Country,
    string Number,
    DateOnly ValidFrom,
    DateOnly ValidUntil,
    Guid? DocumentId,
    string? Note
)
{
    public const string ValidUntilBeforeValidFromMessage =
        "The permission cannot expire before it starts.";

    public bool IsValidOn(DateOnly date) => date >= ValidFrom && date <= ValidUntil;
}
