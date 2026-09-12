using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using Wolverine;
namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal sealed record CreateUserForOrganizationRequest(
    string Email,
    string FirstName,
    string LastName,
    OrganizationRoleApi Role,
    Guid OrganizationId,
    string Password)
{
    internal CreateUser ToCommand()
    {
        return new CreateUser(OrganizationId, Email, FirstName, LastName, Role.ToDomainRole(), Password, Guid.Empty);
    }
}
    

internal static class MapCreateUser
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("users", Handler)
            .WithSummary("Create user")
            .WithName("Create organization user")
            .Produces<UserCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(OwnerAuthenticated user, IMessageBus bus, CreateUserForOrganizationRequest request)
    {
        var result = await bus.InvokeAsync<UserCreated>(request.ToCommand());
        
        return TypedResults.Created($"/api/users/{result.UserId}", UserProjection.Create(result));
    }
}