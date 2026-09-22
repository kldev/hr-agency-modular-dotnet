using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.Avatar.Change;

/// <summary>
/// The bytes are already in storage by the time this is sent - a command travels through the outbox
/// and cannot carry a stream, so the endpoint resolves the file first and passes only its id.
/// <para>
/// <see cref="UserId"/> is whose picture this is and <see cref="ModifiedBy"/> is who is changing it.
/// They are the same person when somebody edits their own profile and differ when an administrator
/// sets a picture for somebody else - which is why the actor is passed in rather than derived from
/// the target, the same shape <c>UpdateUser</c> and <c>ChangeRole</c> already use.
/// </para>
/// </summary>
public sealed record ChangeUserAvatar(
    Guid UserId,
    OrganizationId OrganizationId,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    Guid ModifiedBy
) : IUpdateCommand;

/// <summary>
/// What the endpoint needs back: the picture that is now current, and the one it displaced, whose
/// bytes nobody will ask for again.
/// </summary>
public sealed record UserAvatarChanged(Guid UserId, Guid FileId, Guid? PreviousFileId);
