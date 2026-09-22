using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.ChangeRole;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal sealed record ChangeUserRoleRequest(
    [property: Description(
        "The account's new role in the agency, e.g. Admin, Recruiter, HumanResources, Finance. It takes effect at the person's next sign-in or token refresh."
    )]
        OrganizationRoleApi Role
)
{
    internal ChangeRole ToCommand(OrganizationId organizationId, Guid userId, Guid modifiedBy)
    {
        return new ChangeRole(userId, organizationId, Role.ToDomainRole(), modifiedBy);
    }
}

internal static class MapChangeRole
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Users.ChangeRole, Handler)
            .WithSummary("Change user role")
            .WithName("Change user role")
            .Produces<RoleChanged>()
            .ProducesStandardErrors()
            .RequireAuthorization(AdminPolicy.Name);
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid userId,
        ChangeUserRoleRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<RoleChanged>(
            request.ToCommand(user.GetOrganization, userId, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}
