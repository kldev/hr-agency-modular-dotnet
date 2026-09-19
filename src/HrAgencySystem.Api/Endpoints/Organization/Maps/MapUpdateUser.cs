using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Update;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal sealed record UpdateUserForOrganizationRequest(
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    string? JobTitle = null,
    string? Phone = null
)
{
    internal UpdateUser ToCommand(Guid userId)
    {
        return new UpdateUser(
            userId,
            SharedKernel.Tenant.OrganizationId.From(OrganizationId),
            new ContactPerson(
                Email,
                FirstName,
                LastName,
                JobTitle ?? string.Empty,
                Phone ?? string.Empty
            ),
            Guid.Empty
        );
    }
}

internal static class MapUpdateUser
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Organizations.UpdateUser, Handler)
            .WithSummary("Update user")
            .WithName("Update organization user")
            .Produces<UserUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        OwnerAuthenticated owner,
        IMessageBus bus,
        Guid userId,
        UpdateUserForOrganizationRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<UserUpdated>(request.ToCommand(userId), ct);

        return TypedResults.Ok(result);
    }
}
