namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// What kind of permission to be here and to work a person holds. Open ended on purpose: a value
/// costs nothing, and the alternative - free text - makes "whose permit expires next month"
/// impossible to ask, which is the one question legalisation actually has.
/// </summary>
public enum WorkAuthorisationKind
{
    WorkPermit,
    ResidencePermit,
    Visa,

    /// <summary>A statement entrusting work to a foreigner, where that is what the country uses instead of a permit.</summary>
    WorkStatement,
    Other,
}
