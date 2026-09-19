using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Application.Contacts.Create;

public sealed record CreateCompanyContact(
    Guid OrganizationId,
    Guid CompanyId,
    ContactPerson Contact,
    Guid CreatedBy,
    bool UpdatePrimary = false
) : ICreateCommand, IContactData;
