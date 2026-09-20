using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.Avatar.Change;

/// <summary>
/// The bytes are already in storage by the time this is sent - a command travels through the outbox
/// and cannot carry a stream, so the endpoint resolves the file first and passes only its id.
/// </summary>
public sealed record ChangeUserAvatar(
    Guid UserId,
    OrganizationId OrganizationId,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size
) : IUpdateCommand
{
    public Guid ModifiedBy => UserId;
}

/// <summary>
/// What the endpoint needs back: the picture that is now current, and the one it displaced, whose
/// bytes nobody will ask for again.
/// </summary>
public sealed record UserAvatarChanged(Guid UserId, Guid FileId, Guid? PreviousFileId);
