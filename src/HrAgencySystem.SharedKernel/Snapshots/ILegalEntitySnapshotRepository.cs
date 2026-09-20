using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.SharedKernel.Snapshots;

/// <summary>
/// One of the agency's own companies, as seen by a module that does not own them. Distinct from
/// <see cref="CompanySnapshot"/>, which is the client being invoiced.
/// </summary>
public interface ILegalEntitySnapshotRepository
{
    public const string NotFoundMessage = "Required legal entity data not found.";

    /// <summary>
    /// Resolves an entity only when it belongs to the given organization: a project may only ever be
    /// delivered by one of its own agency's companies.
    /// </summary>
    Task<LegalEntitySnapshot?> GetLegalEntityAsync(
        Guid legalEntityId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}

/// <summary>
/// Everything a consumer needs to name the entity on a contract or a declaration. The registered
/// address is not optional here, unlike on the client side: an entity of ours without one could not
/// sign anything, so it is required when the entity is recorded.
/// </summary>
public sealed record LegalEntitySnapshot(
    Guid Id,
    string Name,
    string LegalName,
    string TaxId,
    string? VatNumber,
    PostalAddress RegisteredAddress,
    DateOnly ActiveFrom,
    DateOnly? ActiveTo
)
{
    /// <summary>Whether the entity was still trading on the given day.</summary>
    public bool IsActiveOn(DateOnly date) =>
        date >= ActiveFrom && (ActiveTo is null || date <= ActiveTo);
}
