using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.ChangePassword;

public sealed record ChangeUserPassword(
    Guid UserId,
    OrganizationId OrganizationId,
    string CurrentPassword,
    string NewPassword
) : IUpdateCommand
{
    public Guid ModifiedBy => UserId;
}
