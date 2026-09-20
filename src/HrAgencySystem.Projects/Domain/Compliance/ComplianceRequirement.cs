namespace HrAgencySystem.Projects.Domain.Compliance;

/// <summary>
/// One formal obligation a project can carry, named after the legal instrument behind it.
/// <para>
/// The law lives in this enum and in <see cref="ComplianceCatalogue"/>, never in control flow. There
/// is no <c>if (country == …)</c> anywhere in this module: a country is a key in a dictionary, so
/// adding France is a row and some values rather than a change to the domain.
/// </para>
/// <para>
/// A country prefix marks a national requirement. The two without one come from EU law and mean the
/// same thing in both countries. Only requirements confirmed against an official source are here -
/// anything found on a commercial site stayed in the plan as a note.
/// </para>
/// </summary>
public enum ComplianceRequirement
{
    /// <summary>
    /// A1 certificates obtained for the posted people. Reg. (EC) 883/2004, art. 12 / 13.
    /// <para>
    /// A project level confirmation, not a register: A1 is issued per person and this system has no
    /// worker entity yet. The label has to say so, because an item pretending to be a register would
    /// give a sense of control that is not there.
    /// </para>
    /// </summary>
    A1Certificates,

    /// <summary>
    /// The user undertaking's statement of the working conditions it applies.
    /// Directive 96/71/EC art. 3(1b), transposed by § 15a AEntG in Germany.
    /// </summary>
    UserConditionsReceived,

    /// <summary>Limosa declaration and the L-1 certificate. Programme Act of 27 December 2006, art. 137-142.</summary>
    BeLimosaDeclaration,

    /// <summary>Liaison person notified in Limosa. Art. 7/2 of the law of 5 March 2002; RD of 20 March 2007, art. 4 §1 10°.</summary>
    BeLiaisonPerson,

    /// <summary>The joint committee covering the activity carried out in Belgium.</summary>
    BeJointCommittee,

    /// <summary>
    /// The user's joint committee, which is what the agency worker's pay is measured against
    /// (art. 10 of the law of 24 July 1987). A separate item from <see cref="BeJointCommittee"/>
    /// because they are different facts and one field for both would quietly hide the second.
    /// </summary>
    BeUserJointCommittee,

    /// <summary>Regional recognition as a temporary work agency. Required before hiring anybody out to a Belgian user.</summary>
    BeTemporaryAgencyRecognition,

    /// <summary>Exemption from Belgian social documents for the first 12 months. RD no. 5 of 23 October 1978, arts. 6quinquies and 6sexies.</summary>
    BeSocialDocumentsExemption,

    /// <summary>Motivated notification extending a posting from 12 to 18 months. Art. 5 of the law of 5 March 2002.</summary>
    BeMotivatedNotification,

    /// <summary>Posting notification under § 18 AEntG. Only for the sectors the customs administration supervises.</summary>
    DeAentgNotification,

    /// <summary>Licence to hire out workers. §§ 1 and 2 AÜG, issued by the Federal Employment Agency.</summary>
    DeAuegPermit,

    /// <summary>Notification before each hiring out. § 17b AÜG.</summary>
    DeAuegNotification,

    /// <summary>Authorised recipient with an address in Germany. § 18(1) no. 5 AEntG, § 17b(1) no. 5 AÜG.</summary>
    DeAuthorisedRecipient,

    /// <summary>Documents kept in German and available in Germany. § 19(2) AEntG, § 17c(2) AÜG.</summary>
    DeDocumentRetention,

    /// <summary>The branch that decides which minimum wage applies. § 4 AEntG, § 3a AÜG.</summary>
    DeBranchDetermination,

    /// <summary>Hiring out agreement in text form, designated as such before the work starts. §§ 12 and 1(1) AÜG.</summary>
    DeUeberlassungAgreement,

    /// <summary>The construction sector restriction on hiring out. § 1b AÜG.</summary>
    DeConstructionSectorRestriction,

    /// <summary>Notification for a posting beyond 12 months. § 13b AEntG.</summary>
    DeLongTermPostingNotification,
}
