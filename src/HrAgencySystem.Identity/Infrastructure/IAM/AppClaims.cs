using System.Security.Claims;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

public static class AppClaims
{
    public const string UserId = ClaimTypes.NameIdentifier;
    public const string Email = ClaimTypes.Email;
    public const string FullName = ClaimTypes.Name;
    public const string Role = ClaimTypes.Role;
    public const string OrganizationId = "organizationId";

    /// <summary>
    /// The administrator behind a session that is not their own. Absent on an ordinary token, which
    /// is what makes "is somebody standing in for this person" a question about the claim existing.
    /// </summary>
    public const string ImpersonatedBy = "impersonatedBy";
}
