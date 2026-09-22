using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Create;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using HrAgencySystem.Teams.Contracts;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.User.Maps;

internal sealed record CreateUserRequest(
    [property: Description("Sign-in e-mail address. Unique within the agency.")] string Email,
    [property: Description("First name.")] string FirstName,
    [property: Description("Last name.")] string LastName,
    [property: Description(
        "What the account may do in the agency, e.g. Admin, Recruiter, HumanResources, Finance."
    )]
        OrganizationRoleApi Role,
    [property: Description(
        "Initial password, at least 4 characters. The person can change it after signing in."
    )]
        string Password,
    [property: Description("Optional job title, e.g. \"Senior recruiter\".")]
        string? JobTitle = null,
    [property: Description("Optional phone number.")] string? Phone = null,
    [property: Description(
        "Optional recruitment team to put the person in straight away. Requires TeamRole."
    )]
        Guid? TeamId = null,
    [property: Description(
        "The person's seat in that team - Sales, Recruiter, Operations or Lead. Only with TeamId."
    )]
        TeamRole? TeamRole = null
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
            createdBy,
            TeamId,
            TeamRole
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
            .ProducesStandardErrors()
            .RequireAuthorization(AdminPolicy.Name);
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
