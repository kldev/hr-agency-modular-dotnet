using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Domain;

/// <summary>
/// The paperwork half of a company: what a contract, an invoice or a posting declaration needs, as
/// opposed to what a sales pipeline needs.
/// <para>
/// Every field is optional and none of it is asked for when a lead is created. A company enters this
/// system as a name on a business card; demanding a registered address there would kill the one
/// thing the sales pipeline has to be - fast to add to.
/// </para>
/// </summary>
public sealed record CompanyProfile(
    string? LegalName,
    PostalAddress? RegisteredAddress,
    string? VatNumber,
    string? Iban,
    string? Bic,
    ContactPerson? LegalRepresentative
)
{
    public static CompanyProfile Empty { get; } = new(null, null, null, null, null, null);

    /// <summary>
    /// What it takes to name a party on a contract: who they legally are and where they sit.
    /// Computed rather than stored, so it cannot go on claiming completeness after somebody clears
    /// the address.
    /// </summary>
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(LegalName) && RegisteredAddress is not null;
}
