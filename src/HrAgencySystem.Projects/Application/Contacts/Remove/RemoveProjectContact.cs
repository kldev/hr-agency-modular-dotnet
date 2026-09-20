using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Contacts.Remove;

public sealed record RemoveProjectContact(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    ContactRole Role,
    Guid ModifiedBy
) : IUpdateCommand;
