namespace HrAgencySystem.Workers.Domain;

public enum IdentityDocumentKind
{
    IdentityCard,
    Passport,

    /// <summary>A residence card, which for many people is the document they are identified by here.</summary>
    ResidenceCard,
    Other,
}
