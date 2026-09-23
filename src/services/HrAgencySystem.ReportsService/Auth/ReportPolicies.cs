using HrAgencySystem.ReportsService.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace HrAgencySystem.ReportsService.Auth;

/// <summary>
/// The two doors of this service. A valid token that is the wrong kind is a 403, decided here
/// before a query runs, rather than an empty report.
/// </summary>
public static class ReportPolicies
{
    /// <summary>An organization's own report: the token must name the organization.</summary>
    public const string Organization = "organization-report";

    /// <summary>The platform owner's report: the token must carry the platform scope.</summary>
    public const string Platform = "platform-report";

    public static void Add(AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            Organization,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        Guid.TryParse(
                            context.User.FindFirst(ReportsServiceToken.OrganizationClaim)?.Value,
                            out _
                        )
                    )
        );

        builder.AddPolicy(
            Platform,
            policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(ReportsServiceToken.ScopeClaim, ReportsServiceToken.PlatformScope)
        );
    }
}
