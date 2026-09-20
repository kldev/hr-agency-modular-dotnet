using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.Avatar.Remove;

public sealed record RemoveUserAvatar(Guid UserId, OrganizationId OrganizationId) : IUpdateCommand
{
    public Guid ModifiedBy => UserId;
}

public sealed record UserAvatarRemoved(Guid UserId, Guid FileId);
