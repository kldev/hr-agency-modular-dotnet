using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.ChangeRole;

public sealed record ChangeRole(
    Guid UserId,
    OrganizationId OrganizationId,
    OrganizationRole Role,
    Guid ModifiedBy
) : IUpdateCommand;
