using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.Avatar.Remove;

/// <summary>
/// Whose picture goes, and who is taking it away - the two differ when an administrator removes it
/// for somebody else. See <c>ChangeUserAvatar</c> for why the actor is passed rather than derived.
/// </summary>
public sealed record RemoveUserAvatar(Guid UserId, OrganizationId OrganizationId, Guid ModifiedBy)
    : IUpdateCommand;

public sealed record UserAvatarRemoved(Guid UserId, Guid FileId);
