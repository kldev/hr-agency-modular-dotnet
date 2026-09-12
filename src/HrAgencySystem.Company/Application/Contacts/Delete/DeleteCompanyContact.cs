using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Company.Application.Contacts.Delete;

public sealed record DeleteCompanyContact(
    Guid OrganizationId, 
    Guid ContactId,
    Guid ModifiedBy) : IUpdateCommand;

public sealed record CompanyContactDeleted(
    Guid ContactId,
    DateTimeOffset DeletedAt
);
    