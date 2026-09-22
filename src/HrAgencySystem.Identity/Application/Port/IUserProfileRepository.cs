using HrAgencySystem.Identity.Documents;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Port;

/// <summary>
/// The profile document, always reached through its owner. The organization is part of every lookup
/// for the same reason it is everywhere else: a row that belongs to another tenant is reported as
/// absent, never as refused.
/// </summary>
public interface IUserProfileRepository
{
    Task<UserProfile?> GetAsync(OrganizationId organizationId, UserId userId, CancellationToken ct);

    /// <summary>
    /// Every profile in the organization that has a picture. Answers "who has one" for a list of
    /// people in one read, instead of a request per row that mostly comes back as a 404.
    /// <para>
    /// Not paged: an agency has tens of employees, and the rows are a handful of columns each. A
    /// tenant with thousands of them would have to narrow this to the page being displayed.
    /// </para>
    /// </summary>
    Task<IReadOnlyList<UserProfile>> GetManyAsync(
        OrganizationId organizationId,
        CancellationToken ct
    );

    /// <summary>
    /// Stores the new picture and answers with the file it replaced, if any. The caller needs that
    /// id to delete the bytes left behind - which is why this returns it rather than swallowing it.
    /// </summary>
    Task<Guid?> SetAvatarAsync(UserProfile profile, CancellationToken ct);

    /// <summary>Drops the row and answers with the file that was on it, or null if there was none.</summary>
    Task<Guid?> RemoveAvatarAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken ct
    );
}
