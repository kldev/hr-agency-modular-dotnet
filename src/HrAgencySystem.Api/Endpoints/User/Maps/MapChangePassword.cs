using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.ChangePassword;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal sealed record ChangePasswordRequest(
    [property: Description(
        "The password in use now - proof that the person at the keyboard owns the account."
    )]
        string CurrentPassword,
    [property: Description(
        "The password to switch to: at least 4 characters and different from the current one. Every session of the account is signed out, this one included."
    )]
        string NewPassword
)
{
    internal ChangeUserPassword ToCommand(OrganizationId organizationId, Guid userId)
    {
        return new ChangeUserPassword(userId, organizationId, CurrentPassword, NewPassword);
    }
}

internal static class MapChangePassword
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Users.ChangePassword, Handler)
            .WithSummary("Change own password")
            .WithName("Change own password")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        ChangePasswordRequest request,
        CancellationToken ct
    )
    {
        await bus.InvokeAsync<PasswordChanged>(
            request.ToCommand(user.GetOrganization, user.UserId),
            ct
        );

        return TypedResults.NoContent();
    }
}
