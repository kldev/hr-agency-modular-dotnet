using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.ChangePassword;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword)
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
