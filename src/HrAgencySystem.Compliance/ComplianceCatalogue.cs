using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Compliance;

/// <summary>
/// Which requirements an engagement carries, as data keyed by country and engagement type.
/// <para>
/// Both halves of the key matter and that is the point. Posting an IT specialist to a German client
/// triggers almost nothing, because IT is on none of the sector lists the customs administration
/// supervises; hiring the same person out to a German user undertaking triggers a licence, a
/// notification and document duties, because hiring out is itself one of those sectors regardless of
/// where the worker ends up; employing them under German law triggers a third set again, and none of
/// the posting ones. A catalogue keyed on country alone would state something untrue.
/// </para>
/// <para>
/// Poland has no entry, and that is the correct answer rather than missing data: posting inside the
/// country of establishment does not raise a host state's obligations.
/// </para>
/// </summary>
public static class ComplianceCatalogue
{
    private static readonly Dictionary<
        (string Country, EngagementType Engagement),
        IReadOnlyList<ComplianceRequirement>
    > Requirements = new()
    {
        [("BE", EngagementType.PostingOfWorkers)] = BePosting,
        [("BE", EngagementType.Outsourcing)] =
        [
            .. BePosting,
            ComplianceRequirement.BeProhibitedPlacement,
        ],
        [("BE", EngagementType.TemporaryAgencyWork)] =
        [
            ComplianceRequirement.A1Certificates,
            ComplianceRequirement.UserConditionsReceived,
            ComplianceRequirement.BeTemporaryAgencyRecognition,
            ComplianceRequirement.BeLimosaDeclaration,
            ComplianceRequirement.BeLiaisonPerson,
            ComplianceRequirement.BeJointCommittee,
            ComplianceRequirement.BeUserJointCommittee,
            ComplianceRequirement.BeSocialDocumentsExemption,
            ComplianceRequirement.BeMotivatedNotification,
        ],
        [("BE", EngagementType.LocalEmployment)] =
        [
            ComplianceRequirement.LocalEmploymentContract,
            ComplianceRequirement.BeDimonaDeclaration,
        ],
        [("DE", EngagementType.PostingOfWorkers)] = DePosting,
        [("DE", EngagementType.Outsourcing)] =
        [
            .. DePosting,
            ComplianceRequirement.DeServiceContractDelimitation,
        ],
        [("DE", EngagementType.TemporaryAgencyWork)] =
        [
            ComplianceRequirement.A1Certificates,
            ComplianceRequirement.UserConditionsReceived,
            ComplianceRequirement.DeAuegPermit,
            ComplianceRequirement.DeAuegNotification,
            ComplianceRequirement.DeAuthorisedRecipient,
            ComplianceRequirement.DeDocumentRetention,
            ComplianceRequirement.DeBranchDetermination,
            ComplianceRequirement.DeUeberlassungAgreement,
            ComplianceRequirement.DeConstructionSectorRestriction,
            ComplianceRequirement.DeLongTermPostingNotification,
        ],
        [("DE", EngagementType.LocalEmployment)] =
        [
            ComplianceRequirement.LocalEmploymentContract,
            ComplianceRequirement.DeSocialSecurityRegistration,
        ],
    };

    /// <summary>
    /// Outsourcing is a posting carried out under our own direction, so it carries the posting
    /// duties and one more: showing that the client is not in fact directing the work.
    /// </summary>
    private static IReadOnlyList<ComplianceRequirement> BePosting =>
        [
            ComplianceRequirement.A1Certificates,
            ComplianceRequirement.BeLimosaDeclaration,
            ComplianceRequirement.BeLiaisonPerson,
            ComplianceRequirement.BeJointCommittee,
            ComplianceRequirement.BeSocialDocumentsExemption,
            ComplianceRequirement.BeMotivatedNotification,
        ];

    private static IReadOnlyList<ComplianceRequirement> DePosting =>
        [
            ComplianceRequirement.A1Certificates,
            ComplianceRequirement.DeAentgNotification,
            ComplianceRequirement.DeAuthorisedRecipient,
            ComplianceRequirement.DeDocumentRetention,
            ComplianceRequirement.DeBranchDetermination,
            ComplianceRequirement.DeLongTermPostingNotification,
        ];

    /// <summary>
    /// Requirements issued to one named person for one period, rather than held by the delivering
    /// entity for everybody. The split is here, as data, for the same reason the countries are: it
    /// is a fact about each instrument, not a branch in anybody's code.
    /// <para>
    /// Two German notifications deliberately stay at project level although they name the people
    /// they cover: § 18 AEntG and § 17b AÜG are one filing per deployment carrying one reference
    /// number, and splitting them per person would copy that same number onto every row.
    /// </para>
    /// </summary>
    private static readonly HashSet<ComplianceRequirement> PerPerson =
    [
        ComplianceRequirement.A1Certificates,
        ComplianceRequirement.BeLimosaDeclaration,
        ComplianceRequirement.BeSocialDocumentsExemption,
        ComplianceRequirement.BeMotivatedNotification,
        ComplianceRequirement.DeLongTermPostingNotification,
        ComplianceRequirement.LocalEmploymentContract,
        ComplianceRequirement.DeSocialSecurityRegistration,
        ComplianceRequirement.BeDimonaDeclaration,
    ];

    public static ComplianceScope ScopeOf(ComplianceRequirement requirement) =>
        PerPerson.Contains(requirement) ? ComplianceScope.Assignment : ComplianceScope.Project;

    /// <summary>
    /// What the given engagement owes at the given level. There is deliberately no overload without
    /// a scope: asking the catalogue for "everything" is how a per person certificate ended up as a
    /// single tick on a project in the first place.
    /// </summary>
    public static IReadOnlyList<ComplianceRequirement> For(
        CountryCode country,
        EngagementType engagement,
        ComplianceScope scope
    ) => For(country.Value, engagement, scope);

    public static IReadOnlyList<ComplianceRequirement> For(
        string country,
        EngagementType engagement,
        ComplianceScope scope
    ) =>
        [
            .. Requirements
                .GetValueOrDefault(((country ?? "").Trim().ToUpperInvariant(), engagement), [])
                .Where(requirement => ScopeOf(requirement) == scope),
        ];

    /// <summary>
    /// Requirements that exist as a numbered thing somewhere - a declaration id, a permit number, a
    /// committee code. Confirming one of these without writing the number down would record that
    /// somebody remembers doing it, which is not the same as being able to prove it.
    /// </summary>
    private static readonly HashSet<ComplianceRequirement> Numbered =
    [
        ComplianceRequirement.BeLimosaDeclaration,
        ComplianceRequirement.BeJointCommittee,
        ComplianceRequirement.BeUserJointCommittee,
        ComplianceRequirement.BeTemporaryAgencyRecognition,
        ComplianceRequirement.BeDimonaDeclaration,
        ComplianceRequirement.DeAuegPermit,
        ComplianceRequirement.DeAuegNotification,
        ComplianceRequirement.DeAentgNotification,
        ComplianceRequirement.DeBranchDetermination,
        ComplianceRequirement.DeSocialSecurityRegistration,
    ];

    public static bool RequiresReferenceNumber(ComplianceRequirement requirement) =>
        Numbered.Contains(requirement);

    public static bool Contains(
        string country,
        EngagementType engagement,
        ComplianceScope scope,
        ComplianceRequirement requirement
    ) => For(country, engagement, scope).Contains(requirement);
}
