using HrAgencySystem.Company.Domain;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Application.Create;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record CreateCompany(
    Guid OrganizationId,
    string Name,
    string CountryCode,
    string TaxId,
    string RegistrationNumber,
    Guid CreatedBy,
    Industry Industry = Industry.Other,
    string WebSite = "",
    ContactPerson? Contact = null
) : ICreateCommand, ICompanyData;