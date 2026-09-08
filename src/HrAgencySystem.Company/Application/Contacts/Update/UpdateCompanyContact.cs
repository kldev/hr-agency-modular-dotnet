using HrAgencySystem.Company.Application.Contacts.Create;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Company.Application.Contacts.Update;

public sealed record UpdateCompanyContact(
    Guid OrganizationId, 
    Guid ContactId,
    string Email,
    string FirstName,
    string LastName,
    string JobTitle,
    Guid ModifiedBy) : IUpdateCommand, IContactData;