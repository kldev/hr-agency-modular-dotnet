namespace HrAgencySystem.Sales.Domain.Activity;

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
