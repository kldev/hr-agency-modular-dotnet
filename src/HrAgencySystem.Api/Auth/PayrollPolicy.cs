using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.AspNetCore.Authorization;

namespace HrAgencySystem.Api.Auth;

/// <summary>
/// The first place an <see cref="OrganizationRole"/> actually decides anything. Until now only
/// <c>PlatformRole.Owner</c> was enforced and the organization roles were labels; settling a month
/// is a job somebody holds, so it needs one.
/// <para>
/// Deliberately narrow. Approving a month is <em>not</em> covered by a role - that follows from the
/// chart, and a role that could approve anybody's hours would make the structure decorative. This
/// only covers what payroll does: taking agreed hours off the supervisors' hands.
/// </para>
/// </summary>
public static class PayrollPolicy
{
    public const string Name = "Payroll";

    /// <summary>
    /// Admin is here because a small agency has no payroll desk and somebody still has to close the
    /// month. It is not a bypass of the supervisor rule - that one has no role-shaped door at all.
    /// </summary>
    private static readonly OrganizationRole[] Allowed =
    [
        OrganizationRole.HumanResources,
        OrganizationRole.Admin,
    ];

    public static bool IsPayroll(OrganizationRole role) => Allowed.Contains(role);

    public static void AddPayrollPolicy(this AuthorizationBuilder builder) =>
        builder.AddPolicy(
            Name,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AppClaims.Role, [.. Allowed.Select(role => role.ToString())])
        );
}
