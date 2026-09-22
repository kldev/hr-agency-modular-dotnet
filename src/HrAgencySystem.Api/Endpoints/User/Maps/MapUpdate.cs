using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Update;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal sealed record UpdateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? JobTitle = null,
    string? Phone = null
)
{
    internal UpdateUser ToCommand(OrganizationId organizationId, Guid userId, Guid modifiedBy)
    {
        return new UpdateUser(
            userId,
            organizationId,
            new ContactPerson(
                Email,
                FirstName,
                LastName,
                JobTitle ?? string.Empty,
                Phone ?? string.Empty
            ),
            modifiedBy
        );
    }
}

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Users.Update, Handler)
            .WithSummary("Update user")
            .WithName("Update user")
            .Produces<UserUpdated>()
            .ProducesStandardErrors()
            .RequireAuthorization(AdminPolicy.Name);
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid userId,
        UpdateUserRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<UserUpdated>(
            request.ToCommand(user.GetOrganization, userId, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}
