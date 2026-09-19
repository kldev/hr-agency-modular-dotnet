using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal sealed record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    OrganizationRoleApi Role,
    string Password,
    string? JobTitle = null,
    string? Phone = null
)
{
    internal CreateUser ToCommand(OrganizationId organizationId, Guid createdBy)
    {
        return new CreateUser(
            organizationId.Value,
            new ContactPerson(
                Email,
                FirstName,
                LastName,
                JobTitle ?? string.Empty,
                Phone ?? string.Empty
            ),
            Role.ToDomainRole(),
            Password,
            createdBy
        );
    }
}

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Users.Create, Handler)
            .WithSummary("Create user")
            .WithName("Create user")
            .Produces<UserCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        CreateUserRequest request
    )
    {
        var result = await bus.InvokeAsync<UserCreated>(
            request.ToCommand(user.GetOrganization, user.UserId)
        );

        return TypedResults.Created($"/api/users/{result.UserId}", UserProjection.Create(result));
    }
}
