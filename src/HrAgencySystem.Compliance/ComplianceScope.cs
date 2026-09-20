namespace HrAgencySystem.Compliance;

/// <summary>
/// Who carries a requirement: the engagement as a whole, or one named person for one period.
/// <para>
/// The distinction is not administrative. An AÜG licence is held by the company and covers every
/// person hired out under it; an A1 certificate names one person, one period and one posting entity,
/// and the next person needs their own. Recording the second kind once per project states that
/// everybody is covered, which is a claim nobody checked - that is precisely the gap this scope
/// closes.
/// </para>
/// </summary>
public enum ComplianceScope
{
    /// <summary>Held by the delivering entity or agreed with the client. One answer for everybody.</summary>
    Project,

    /// <summary>Issued to one person for one period. A register, not a tick.</summary>
    Assignment,
}
