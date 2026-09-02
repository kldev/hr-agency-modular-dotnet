using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal sealed record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role,
    string Password)
{
    internal CreateUser ToCommand(OrganizationId organizationId)
    {
        return new CreateUser(organizationId.Value, Email, FirstName, LastName, Role, Password);
    }
}

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/api/users", Handler)
            .WithSummary("Create user")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest);;
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, IMessageBus bus, CreateUserRequest request)
    {
        var result = await bus.InvokeAsync<UserCreated>(request.ToCommand(user.GetOrganization));
        
        return TypedResults.Created($"/api/users/{result.UserId}", UserProjection.Create(result));
    }
}