using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Company.Application.Contacts.Create;

public sealed record CreateCompanyContact(
    Guid OrganizationId, 
    Guid CompanyId,
    string Email,
    string FirstName,
    string LastName,
    string JobTitle,
    Guid CreatedBy) : ICreateCommand, IContactData;