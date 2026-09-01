using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Api.Auth;

public record AuthenticatedUser(Guid UserId, string Email, Guid OrganizationId, OrganizationRole Role)
{
    public static ValueTask<AuthenticatedUser?> BindAsync(
        HttpContext context)
    {
        var user = context.User.GetAuthenticatedUser();

        return ValueTask.FromResult<AuthenticatedUser?>(user);
    }

    public OrganizationId GetOrganization => new (OrganizationId);
}

public sealed record AuthenticatedOwner(string Email, PlatformRole Role)
{
    public static ValueTask<AuthenticatedOwner?> BindAsync(
        HttpContext context)
    {
        var user = context.User.GetOwner();

        return ValueTask.FromResult<AuthenticatedOwner?>(user);
    }
}