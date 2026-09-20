using System.Reflection;
using System.Security.Claims;
using HrAgencySystem.FileService.Contracts;

namespace HrAgencySystem.FileService.Auth;

/// <summary>
/// Who is calling, taken from the signed token and never from the request. The organization is not a
/// parameter anywhere in this service on purpose: a parameter can be swapped, a claim is signed.
/// </summary>
public sealed record ServiceCaller(Guid OrganizationId, Guid ActorId)
{
    public static ValueTask<ServiceCaller?> BindAsync(HttpContext context, ParameterInfo _)
    {
        var organizationId = ReadGuid(context.User, FileServiceToken.OrganizationClaim);
        if (organizationId is null)
            throw new UnauthorizedAccessException(
                $"The service token carries no usable '{FileServiceToken.OrganizationClaim}' claim."
            );

        // The actor is only stamped on the upload record, so an unreadable one is not worth
        // refusing the call over.
        var actorId = ReadGuid(context.User, FileServiceToken.ActorClaim) ?? Guid.Empty;

        return ValueTask.FromResult<ServiceCaller?>(
            new ServiceCaller(organizationId.Value, actorId)
        );
    }

    private static Guid? ReadGuid(ClaimsPrincipal principal, string claimType) =>
        Guid.TryParse(principal.FindFirstValue(claimType), out var value) ? value : null;
}
