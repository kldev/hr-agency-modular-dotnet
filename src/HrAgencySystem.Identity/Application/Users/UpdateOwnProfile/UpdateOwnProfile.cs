using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Application.Users.UpdateOwnProfile;

/// <summary>
/// Editing yourself. There is deliberately no <c>Email</c> here: the address is also the login, and a
/// type with nothing to carry it in cannot be talked into changing it - a guarantee in the shape of
/// the command rather than in the discipline of whoever writes the next endpoint.
/// <para>
/// <see cref="ModifiedBy"/> is the user themselves, because there is no other person this command
/// could ever be about.
/// </para>
/// </summary>
public sealed record UpdateOwnProfile(
    Guid UserId,
    OrganizationId OrganizationId,
    string FirstName,
    string LastName,
    string JobTitle,
    string Phone
) : IUpdateCommand
{
    public Guid ModifiedBy => UserId;
}
