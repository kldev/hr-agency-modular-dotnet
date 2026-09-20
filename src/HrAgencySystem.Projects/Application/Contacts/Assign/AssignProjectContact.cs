using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Web.Common;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Contacts.Assign;

public sealed record AssignProjectContact(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    ContactRole Role,
    ContactPerson Person,
    Guid? CompanyContactId,
    Guid ModifiedBy
) : IUpdateCommand;
