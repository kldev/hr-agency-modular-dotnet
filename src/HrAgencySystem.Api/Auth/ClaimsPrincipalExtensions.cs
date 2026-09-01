using System.Security.Claims;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;

namespace HrAgencySystem.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static AuthenticatedUser GetAuthenticatedUser(
        this ClaimsPrincipal principal)
    {
        var id = principal.FindFirst(AppClaims.UserId)?.Value;
        var email = principal.FindFirst(AppClaims.Email)?.Value;
        var organizationId = principal.FindFirst(AppClaims.OrganizationId)?.Value;

        if (!Guid.TryParse(id, out var userId))
            throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");

        if (!Guid.TryParse(organizationId, out var orgId))
            throw new InvalidOperationException("Organization id claim is missing or invalid.");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Authenticated user email claim is missing.");

        return new AuthenticatedUser(
            userId,
            email,
            orgId,
            principal.GetOrganizationRole());
    }

    public static AuthenticatedOwner GetOwner(this ClaimsPrincipal principal)
    {
        var id = principal.FindFirst(AppClaims.UserId)?.Value;
        var email = principal.FindFirst(AppClaims.Email)?.Value;

        if (!Guid.TryParse(id, out var userId))
            throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Authenticated user email claim is missing.");

        return new AuthenticatedOwner(
            email, principal.GetPlatformRole());
    }

    private static OrganizationRole GetOrganizationRole(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirst(AppClaims.Role)?.Value;

        if (!Enum.TryParse<OrganizationRole>(role, ignoreCase: true, out var result))
            throw new UnauthorizedAccessException(
                $"Invalid organization role: '{role}'.");

        return result;
    }
    
    private static PlatformRole GetPlatformRole(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirst(AppClaims.Role)?.Value;

        if (!Enum.TryParse<PlatformRole>(role, ignoreCase: true, out var result))
            throw new UnauthorizedAccessException(
                $"Invalid platform role: '{role}'.");

        return result;
    }
}