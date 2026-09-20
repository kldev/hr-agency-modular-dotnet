using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// The client as the contract names them, frozen when the contract is recorded.
/// <para>
/// This is the one piece of client data that must not follow the company record. A contract states
/// who signed it and under which address; if the company later moves or is renamed, the contract
/// still says what it said. Everything else about the client is read live.
/// </para>
/// </summary>
public sealed record CompanyPartySnapshot(
    string LegalName,
    string TaxId,
    string? VatNumber,
    PostalAddress RegisteredAddress
);
