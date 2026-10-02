using System.Reflection;
using System.Security.Claims;
using HrAgencySystem.ReportsService.Contracts;
using JetBrains.Annotations;

namespace HrAgencySystem.ReportsService.Auth;

/// <summary>
/// The organization a report is for, taken from the signed token. Never a query parameter: a
/// parameter can be swapped, a claim is signed. Only bound behind <see cref="ReportPolicies.Organization"/>.
/// </summary>
public sealed record OrganizationCaller(Guid OrganizationId)
{
    [UsedImplicitly] // Minimal APIs bind the parameter through this static method.
    public static ValueTask<OrganizationCaller?> BindAsync(HttpContext context, ParameterInfo _)
    {
        var claim = context.User.FindFirstValue(ReportsServiceToken.OrganizationClaim);

        return Guid.TryParse(claim, out var organizationId)
            ? ValueTask.FromResult<OrganizationCaller?>(new OrganizationCaller(organizationId))
            : throw new UnauthorizedAccessException(
                $"The service token carries no usable '{ReportsServiceToken.OrganizationClaim}' claim."
            );
    }
}
