using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Web.Common;
using JasperFx;

namespace HrAgencySystem.Company.Application.CompleteProfile;

public sealed record CompleteCompanyProfile(
    [property: Identity] Guid CompanyId,
    Guid OrganizationId,
    string? LegalName,
    string? Street,
    string? BuildingNumber,
    string? UnitNumber,
    string? PostalCode,
    string? City,
    string? CountryCode,
    string? VatNumber,
    string? Iban,
    string? Bic,
    ContactPerson? LegalRepresentative,
    Guid ModifiedBy
) : IUpdateCommand;
