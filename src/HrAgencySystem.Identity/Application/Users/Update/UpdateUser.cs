using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Identity.Application.Users.Update;

public sealed record UpdateUser(
    Guid UserId,
    OrganizationId OrganizationId,
    ContactPerson Contact,
    Guid ModifiedBy
) : IContactData, IUpdateCommand;
