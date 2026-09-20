using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.LegalEntities.Application.Create;

public sealed record CreateLegalEntity(
    Guid OrganizationId,
    string Name,
    string LegalName,
    string TaxId,
    string? VatNumber,
    string Street,
    string BuildingNumber,
    string? UnitNumber,
    string PostalCode,
    string City,
    string CountryCode,
    string? Description,
    string PresidentFirstName,
    string PresidentLastName,
    string? PresidentEmail,
    DateOnly ActiveFrom,
    DateOnly? ActiveTo,
    IReadOnlyList<BankAccountData> BankAccounts,
    Guid CreatedBy
) : ICreateCommand, ILegalEntityData;
