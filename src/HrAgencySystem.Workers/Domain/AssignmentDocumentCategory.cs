namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// What a document about one posting is. Issued for this person, this period and this delivering
/// company - somebody moving to a project run by another of our companies needs new ones, even with
/// the same client.
/// </summary>
public enum AssignmentDocumentCategory
{
    /// <summary>The contract or annex covering this posting.</summary>
    Contract,

    /// <summary>A1 or the local equivalent: which social security system the person stays in.</summary>
    SocialSecurity,

    /// <summary>A filing with the host country - § 18 AEntG, Limosa, Dimona.</summary>
    HostCountryNotification,

    /// <summary>Anything recorded as proof against a compliance requirement.</summary>
    Compliance,
    Other,
}
