using JetBrains.Annotations;

namespace HrAgencySystem.Sales.Domain.Activity;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum SalesActivityType
{
    Call,
    Email,
    Meeting,
    Note,
    Presentation,

    /// <summary>A task of the opportunity was done - written by the tasks module, not by hand.</summary>
    Task,
    Other,
}
