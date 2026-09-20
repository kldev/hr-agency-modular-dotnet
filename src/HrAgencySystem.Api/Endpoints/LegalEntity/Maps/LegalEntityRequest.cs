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
    string Name,
    string LegalName,
    string TaxId,
    string Street,
    string BuildingNumber,
    string PostalCode,
    string City,
    string CountryCode,
    string PresidentFirstName,
    string PresidentLastName,
    DateOnly ActiveFrom,
    IReadOnlyList<BankAccountData> BankAccounts,
    string? VatNumber = null,
    string? UnitNumber = null,
    string? Description = null,
    string? PresidentEmail = null,
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
