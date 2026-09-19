using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal sealed record CreateUserForOrganizationRequest(
    string Email,
    string FirstName,
    string LastName,
    OrganizationRoleApi Role,
    Guid OrganizationId,
    string Password,
    string? JobTitle = null,
    string? Phone = null
)
{
    internal CreateUser ToCommand()
    {
        return new CreateUser(
            OrganizationId,
            new ContactPerson(
                Email,
                FirstName,
                LastName,
                JobTitle ?? string.Empty,
                Phone ?? string.Empty
            ),
            Role.ToDomainRole(),
            Password,
            Guid.Empty
        );
    }
}

internal static class MapCreateUser
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Organizations.CreateUser, Handler)
            .WithSummary("Create user")
            .WithName("Create organization user")
            .Produces<UserCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        OwnerAuthenticated user,
        IMessageBus bus,
        CreateUserForOrganizationRequest request
    )
    {
        var result = await bus.InvokeAsync<UserCreated>(request.ToCommand());

        return TypedResults.Created($"/api/users/{result.UserId}", UserProjection.Create(result));
    }
}
