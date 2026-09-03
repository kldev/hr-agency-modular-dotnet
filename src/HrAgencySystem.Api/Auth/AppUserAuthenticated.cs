using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Api.Auth;

public record AppUserAuthenticated(Guid UserId, string Email, Guid OrganizationId, OrganizationRole Role)
{
    public static ValueTask<AppUserAuthenticated?> BindAsync(
        HttpContext context)
    {
        var user = context.User.GetAuthenticatedUser();

        return ValueTask.FromResult<AppUserAuthenticated?>(user);
    }

    public OrganizationId GetOrganization => new (OrganizationId);
}

public sealed record OwnerAuthenticated(Guid Id, string Email, PlatformRole Role)
{
    public static ValueTask<OwnerAuthenticated?> BindAsync(
        HttpContext context)
    {
        var user = context.User.GetOwner();

        return ValueTask.FromResult<OwnerAuthenticated?>(user);
    }
}