using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Web.Common;
using HrAgencySystem.Teams.Contracts;

namespace HrAgencySystem.Identity.Application.Users.Create;

public sealed record CreateUser(
    Guid OrganizationId,
    ContactPerson Contact,
    OrganizationRole Role,
    string Password,
    Guid CreatedBy,
    Guid? TeamId = null,
    TeamRole? TeamRole = null
) : IContactData;
