using HrAgencySystem.Company.Application.Create;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Company.Application.Update;

public sealed record UpdateCompany(
    Guid CompanyId,
    Guid OrganizationId,
    string Name,
    string RegistrationNumber,
    Industry Industry,
    string WebSite,
    string CountryCode,
    Guid ModifiedBy) : IUpdateCommand, ICompanyData;
