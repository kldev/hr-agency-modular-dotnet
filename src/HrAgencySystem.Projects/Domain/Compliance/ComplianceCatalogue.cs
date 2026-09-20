using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Domain.Compliance;

/// <summary>
/// Which requirements a project carries, as data keyed by country and engagement type.
/// <para>
/// Both halves of the key matter and that is the point. Posting an IT specialist to a German client
/// triggers almost nothing, because IT is on none of the sector lists the customs administration
/// supervises; hiring the same person out to a German user undertaking triggers a licence, a
/// notification and document duties, because hiring out is itself one of those sectors regardless of
/// where the worker ends up. A catalogue keyed on country alone would state something untrue.
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
        [("BE", EngagementType.PostingOfWorkers)] =
        [
            ComplianceRequirement.A1Certificates,
            ComplianceRequirement.BeLimosaDeclaration,
            ComplianceRequirement.BeLiaisonPerson,
            ComplianceRequirement.BeJointCommittee,
            ComplianceRequirement.BeSocialDocumentsExemption,
            ComplianceRequirement.BeMotivatedNotification,
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
        [("DE", EngagementType.PostingOfWorkers)] =
        [
            ComplianceRequirement.A1Certificates,
            ComplianceRequirement.DeAentgNotification,
            ComplianceRequirement.DeAuthorisedRecipient,
            ComplianceRequirement.DeDocumentRetention,
            ComplianceRequirement.DeBranchDetermination,
            ComplianceRequirement.DeLongTermPostingNotification,
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
    };

    public static IReadOnlyList<ComplianceRequirement> For(
        CountryCode country,
        EngagementType engagement
    ) => For(country.Value, engagement);

    public static IReadOnlyList<ComplianceRequirement> For(
        string country,
        EngagementType engagement
    ) =>
        Requirements.GetValueOrDefault(((country ?? "").Trim().ToUpperInvariant(), engagement), []);

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
        ComplianceRequirement.DeAuegPermit,
        ComplianceRequirement.DeAuegNotification,
        ComplianceRequirement.DeAentgNotification,
        ComplianceRequirement.DeBranchDetermination,
    ];

    public static bool RequiresReferenceNumber(ComplianceRequirement requirement) =>
        Numbered.Contains(requirement);

    public static bool Contains(
        string country,
        EngagementType engagement,
        ComplianceRequirement requirement
    ) => For(country, engagement).Contains(requirement);
}
