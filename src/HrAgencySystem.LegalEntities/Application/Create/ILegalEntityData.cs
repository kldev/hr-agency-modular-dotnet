using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.LegalEntities.Application.Create;

/// <summary>One account as a caller states it, before it is known to be a valid one.</summary>
public sealed record BankAccountData(
    BankAccountPurpose Purpose,
    CurrencyCode Currency,
    string Iban,
    string? Bic,
    string? BankName
);

/// <summary>
/// The description of an entity, shared by creating one and editing one so the two cannot drift
/// apart. Flat rather than nested, because it is what an HTTP request carries.
/// </summary>
public interface ILegalEntityData
{
    string Name { get; }
    string LegalName { get; }
    string TaxId { get; }
    string? VatNumber { get; }

    string Street { get; }
    string BuildingNumber { get; }
    string? UnitNumber { get; }
    string PostalCode { get; }
    string City { get; }
    string CountryCode { get; }

    string? Description { get; }

    string PresidentFirstName { get; }
    string PresidentLastName { get; }
    string? PresidentEmail { get; }

    DateOnly ActiveFrom { get; }
    DateOnly? ActiveTo { get; }

    IReadOnlyList<BankAccountData> BankAccounts { get; }
}
