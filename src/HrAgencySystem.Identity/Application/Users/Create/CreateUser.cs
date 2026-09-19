using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Identity.Application.Users.Create;

public sealed record CreateUser(
    Guid OrganizationId,
    ContactPerson Contact,
    OrganizationRole Role,
    string Password,
    Guid CreatedBy
) : IContactData;
