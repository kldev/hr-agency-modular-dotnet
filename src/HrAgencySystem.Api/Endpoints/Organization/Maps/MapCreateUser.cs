using System.ComponentModel;
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
    [property: Description("Sign-in e-mail address. Unique within the agency.")] string Email,
    [property: Description("First name.")] string FirstName,
    [property: Description("Last name.")] string LastName,
    [property: Description(
        "What the account may do in the agency, e.g. Admin, Recruiter, HumanResources, Finance."
    )]
        OrganizationRoleApi Role,
    [property: Description(
        "The agency the account is created in. The platform owner stands above every agency, so it is named here rather than read from a token."
    )]
        Guid OrganizationId,
    [property: Description(
        "Initial password, at least 4 characters. The person can change it after signing in."
    )]
        string Password,
    [property: Description("Optional job title, e.g. \"Senior recruiter\".")]
        string? JobTitle = null,
    [property: Description("Optional phone number.")] string? Phone = null
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
