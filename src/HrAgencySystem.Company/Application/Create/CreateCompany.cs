using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Company.Application.Create;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record CreateCompany(
    Guid OrganizationId,
    string Name,
    string CountryCode,
    string TaxId,
    string RegistrationNumber,
    Guid CreatedBy
) : ICreateCommand;