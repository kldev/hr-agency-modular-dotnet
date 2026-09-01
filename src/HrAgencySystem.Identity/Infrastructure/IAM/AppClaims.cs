using System.Security.Claims;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

public static class AppClaims
{
    public const string UserId = ClaimTypes.NameIdentifier;
    public const string Email = ClaimTypes.Email;
    public const string FullName = ClaimTypes.Name;
    public const string Role = ClaimTypes.Role;
    public const string OrganizationId = "organizationId";
}