using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// Which of our own companies delivers this project, as it stood when the project was set up.
/// <para>
/// A snapshot rather than an id, for the same reason the client is one: the project record has to
/// keep saying who ran the engagement even after the entity is renamed or moves. Distinct from
/// <see cref="CompanyPartySnapshot"/>, which is the client on the other side of the contract.
/// </para>
/// <para>
/// It can be swapped while the project is still a draft. Once the project starts it cannot change
/// at all - carrying on under a different company is a different engagement, with its own contract
/// and its own notifications, which is what copying the project is for.
/// </para>
/// </summary>
public sealed record DeliveringEntitySnapshot(
    Guid LegalEntityId,
    string Name,
    string LegalName,
    string TaxId,
    string? VatNumber,
    PostalAddress RegisteredAddress
);
