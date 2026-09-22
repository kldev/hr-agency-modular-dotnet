using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.AspNetCore.Authorization;

namespace HrAgencySystem.Api.Auth;

/// <summary>
/// Managing the people in an organization - creating them, editing them, handing out roles, and
/// signing in as one of them. The second place an <see cref="OrganizationRole"/> decides anything,
/// after <see cref="PayrollPolicy"/>.
/// <para>
/// It exists because these endpoints used to be open to any authenticated member, which made the
/// role itself meaningless: a recruiter could grant themselves <c>Admin</c> through
/// <c>PUT /api/users/{userId}/role</c>. Gating "log in as somebody" while leaving that door open
/// would have guarded nothing, so both go through here.
/// </para>
/// </summary>
public static class AdminPolicy
{
    public const string Name = "Admin";

    private static readonly OrganizationRole[] Allowed = [OrganizationRole.Admin];

    public static bool IsAdmin(OrganizationRole role) => Allowed.Contains(role);

    public static void AddAdminPolicy(this AuthorizationBuilder builder) =>
        builder.AddPolicy(
            Name,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AppClaims.Role, [.. Allowed.Select(role => role.ToString())])
        );
}
