using HrAgencySystem.Company.Application.Contacts.Create;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Application.Contacts.Update;

public sealed record UpdateCompanyContact(
    Guid OrganizationId, 
    Guid ContactId,
    ContactPerson Contact,
    bool UpdatePrimary,
    Guid ModifiedBy) : IUpdateCommand, IContactData;