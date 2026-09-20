namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// The document a person is identified by. One per file, and it is what tells two people with the
/// same name apart - which is the whole reason this register can claim to be one.
/// <para>
/// The number lives on the aggregate and nowhere else: not in the read model, not in a log line, not
/// in an error message. What a list needs is the kind, the issuing country and when it expires.
/// </para>
/// </summary>
public sealed record IdentityDocument(
    IdentityDocumentKind Kind,
    string Number,
    string IssuingCountry,
    DateOnly? ValidUntil
);
