using System.ComponentModel;
using HrAgencySystem.LegalEntities.Application.Create;
using HrAgencySystem.LegalEntities.Application.Update;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Api.Endpoints.LegalEntity.Maps;

/// <summary>
/// One body for creating and for editing, because a legal entity is described the same way either
/// way. The two commands stay separate - only one of them can start a stream - but the shape a
/// caller has to fill in does not have to be written twice.
/// </summary>
internal sealed record LegalEntityRequest(
    [property: Description(
        "The short name the agency uses for the company, e.g. \"HR Agency Poland\"."
    )]
        string Name,
    [property: Description(
        "The name as registered, up to 250 characters - the one on contracts and posting declarations."
    )]
        string LegalName,
    [property: Description(
        "Tax identification number (NIP in Poland), up to 50 characters. Unique within the agency."
    )]
        string TaxId,
    [property: Description("Street of the registered address.")] string Street,
    [property: Description("Building number of the registered address.")] string BuildingNumber,
    [property: Description("Postal code of the registered address.")] string PostalCode,
    [property: Description("City of the registered address.")] string City,
    [property: Description("Country of registration, ISO 3166-1 alpha-2.")] string CountryCode,
    [property: Description("First name of the person heading the company (the board's president).")]
        string PresidentFirstName,
    [property: Description("Last name of the person heading the company.")]
        string PresidentLastName,
    [property: Description("First day the company trades.")] DateOnly ActiveFrom,
    [property: Description(
        "The company's bank accounts: IBAN (15-34 characters, starting with a country code), BIC (8 or 11 characters), bank name, currency and purpose. One account per purpose and currency."
    )]
        IReadOnlyList<BankAccountData> BankAccounts,
    [property: Description(
        "Optional EU VAT number: a two letter country code followed by 2-12 letters or digits, e.g. \"PL1234567890\"."
    )]
        string? VatNumber = null,
    [property: Description("Optional unit number of the registered address.")]
        string? UnitNumber = null,
    [property: Description("Optional note on what the company is used for.")]
        string? Description = null,
    [property: Description("Optional e-mail address of the person heading the company.")]
        string? PresidentEmail = null,
    [property: Description("Last day the company trades, if already known; not before ActiveFrom.")]
        DateOnly? ActiveTo = null
)
{
    internal CreateLegalEntity ToCreateCommand(OrganizationId organizationId, Guid createdBy)
    {
        return new CreateLegalEntity(
            organizationId.Value,
            Name,
            LegalName,
            TaxId,
            VatNumber,
            Street,
            BuildingNumber,
            UnitNumber,
            PostalCode,
            City,
            CountryCode,
            Description,
            PresidentFirstName,
            PresidentLastName,
            PresidentEmail,
            ActiveFrom,
            ActiveTo,
            BankAccounts,
            createdBy
        );
    }

    internal UpdateLegalEntity ToUpdateCommand(
        OrganizationId organizationId,
        Guid legalEntityId,
        Guid modifiedBy
    )
    {
        return new UpdateLegalEntity(
            legalEntityId,
            organizationId.Value,
            Name,
            LegalName,
            TaxId,
            VatNumber,
            Street,
            BuildingNumber,
            UnitNumber,
            PostalCode,
            City,
            CountryCode,
            Description,
            PresidentFirstName,
            PresidentLastName,
            PresidentEmail,
            ActiveFrom,
            ActiveTo,
            BankAccounts,
            modifiedBy
        );
    }
}
