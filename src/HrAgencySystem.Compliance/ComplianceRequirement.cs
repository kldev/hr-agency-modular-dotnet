namespace HrAgencySystem.Compliance;

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
    /// The A1 certificate covering one posted person for one period. Reg. (EC) 883/2004, art. 12 / 13.
    /// <para>
    /// <see cref="ComplianceScope.Assignment"/>, and that is the whole point of the scope existing:
    /// A1 is issued to a named person by a named posting entity for a named period, so a project
    /// level tick would be a claim about people nobody listed. Somebody who moves to a project run
    /// by another of our companies needs a new one even if the client never changes.
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

    // Appended rather than slotted in beside their relatives: Marten stores an enum as its ordinal,
    // so reordering this list would silently rewrite the meaning of every requirement already
    // recorded. The catalogue, not this order, is what groups them.

    /// <summary>
    /// The service contract is a genuine one: we direct the work and owe a result, rather than
    /// putting people at the client's disposal. § 1(1) sentence 2 AÜG, read with § 611a BGB.
    /// <para>
    /// The only requirement here that is an assessment rather than a filing. It is on the list
    /// because getting it wrong reclassifies the whole engagement as hiring out without a licence -
    /// the one risk outsourcing carries that posting does not.
    /// </para>
    /// </summary>
    DeServiceContractDelimitation,

    /// <summary>
    /// The work does not amount to putting personnel at a user's disposal, which Belgium prohibits
    /// outright outside recognised temporary agency work. Art. 31 of the law of 24 July 1987.
    /// </summary>
    BeProhibitedPlacement,

    /// <summary>
    /// An employment contract governed by the law of the country where the work is done.
    /// Reg. (EC) 593/2008 (Rome I), art. 8.
    /// <para>
    /// Unprefixed because the obligation is the same sentence everywhere, even though the contract
    /// it produces is a different document in each country. This is what local employment has
    /// instead of a posting: there is no A1 to obtain, because nobody left their own system.
    /// </para>
    /// </summary>
    LocalEmploymentContract,

    /// <summary>
    /// Registration of the employee with German social security before the work starts.
    /// § 28a SGB IV (DEÜV notification).
    /// </summary>
    DeSocialSecurityRegistration,

    /// <summary>
    /// The Dimona declaration, filed before the first day of work for anybody employed in Belgium.
    /// Royal Decree of 5 November 2002.
    /// </summary>
    BeDimonaDeclaration,
}
