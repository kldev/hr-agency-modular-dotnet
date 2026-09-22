using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.AspNetCore.Authorization;

namespace HrAgencySystem.Api.Auth;

/// <summary>
/// Who is shown what somebody is paid. A different set from <see cref="PayrollPolicy"/>, because
/// finance handles money without closing anybody's month.
/// <para>
/// A supervisor approving hours is deliberately not here: they agree how long somebody worked, not
/// what it earns, which is the same line that keeps approval free of any role-shaped door.
/// </para>
/// </summary>
public static class RatesPolicy
{
    public const string Name = "Rates";

    /// <summary>Admin for the same reason as in payroll: a small agency has nobody else to ask.</summary>
    private static readonly OrganizationRole[] Allowed =
    [
        OrganizationRole.HumanResources,
        OrganizationRole.Finance,
        OrganizationRole.Admin,
    ];

    public static bool IsRates(OrganizationRole role) => Allowed.Contains(role);

    public static void AddRatesPolicy(this AuthorizationBuilder builder) =>
        builder.AddPolicy(
            Name,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AppClaims.Role, [.. Allowed.Select(role => role.ToString())])
        );
}
