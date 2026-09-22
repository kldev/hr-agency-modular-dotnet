using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.UpdateOwnProfile;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

/// <summary>
/// No e-mail field, on purpose: the address is the login, and this surface must not be able to
/// change it. The command it builds has no room for one either, so there is nothing to smuggle.
/// </summary>
internal sealed record UpdateOwnProfileRequest(
    [property: Description("Your first name.")] string FirstName,
    [property: Description("Your last name.")] string LastName,
    [property: Description("Optional job title. Omitted or null clears it.")]
        string? JobTitle = null,
    [property: Description("Optional phone number. Omitted or null clears it.")]
        string? Phone = null
)
{
    internal UpdateOwnProfile ToCommand(OrganizationId organizationId, Guid userId)
    {
        return new UpdateOwnProfile(
            userId,
            organizationId,
            FirstName,
            LastName,
            JobTitle ?? string.Empty,
            Phone ?? string.Empty
        );
    }
}

internal static class MapUpdateMe
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Users.Me, Handler)
            .WithSummary("Update own profile")
            .WithName("Update own profile")
            .Produces<UserUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        UpdateOwnProfileRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<UserUpdated>(
            request.ToCommand(user.GetOrganization, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}
