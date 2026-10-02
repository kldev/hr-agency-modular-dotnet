using JetBrains.Annotations;

namespace HrAgencySystem.Workers.Domain;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum IdentityDocumentKind
{
    IdentityCard,
    Passport,

    /// <summary>A residence card, which for many people is the document they are identified by here.</summary>
    ResidenceCard,
    Other,
}
