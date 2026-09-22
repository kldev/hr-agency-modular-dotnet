using System.Text.Json.Serialization;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Api.Auth;

/// <param name="ImpersonatedBy">
/// The administrator signed in as this person, or null on an ordinary session. It is read straight
/// off the token like every other field here - whether a session is delegated, and by whom, is a
/// fact about the session, the same shelf as the role and the organization.
/// </param>
public record AppUserAuthenticated(
    Guid UserId,
    string Email,
    Guid OrganizationId,
    OrganizationRole Role,
    string FullName,
    Guid? ImpersonatedBy = null
)
{
    public static ValueTask<AppUserAuthenticated?> BindAsync(HttpContext context)
    {
        var user = context.User.GetAuthenticatedUser();

        return ValueTask.FromResult<AppUserAuthenticated?>(user);
    }

    [JsonIgnore]
    public OrganizationId GetOrganization =>
        SharedKernel.Tenant.OrganizationId.From(OrganizationId);
}

public sealed record OwnerAuthenticated(Guid Id, string Email, PlatformRole Role)
{
    public static ValueTask<OwnerAuthenticated?> BindAsync(HttpContext context)
    {
        var user = context.User.GetOwner();

        return ValueTask.FromResult<OwnerAuthenticated?>(user);
    }
}
