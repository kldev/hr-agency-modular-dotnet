namespace HrAgencySystem.Identity.Documents;

/// <summary>
/// What a person keeps about themselves that the domain has no rule about. A plain document rather
/// than state on the <c>User</c> stream: nobody will ever ask which picture somebody had in March,
/// and an avatar takes part in no invariant, so paying for a stream and an async projection would
/// buy a history nobody reads and a read that lags behind the upload.
/// <para>
/// <see cref="Id"/> is the user's id - that is the whole of the link. No row means no avatar;
/// removing a picture deletes the row instead of blanking its fields, so "has an avatar" stays a
/// question about existence rather than about which columns happen to be empty.
/// </para>
/// <para>
/// Name, e-mail, job title and phone deliberately stay on the <c>User</c> aggregate: <c>UserSnapshot</c>
/// stamps every "created by / modified by" in the application from there, and a second copy here
/// would be a second truth.
/// </para>
/// </summary>
public sealed record UserProfile(
    Guid Id,
    Guid OrganizationId,
    Guid AvatarFileId,
    string AvatarFileName,
    string AvatarContentType,
    long AvatarSize,
    DateTimeOffset AvatarUploadedAt
);
