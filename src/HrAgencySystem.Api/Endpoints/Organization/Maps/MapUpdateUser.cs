using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Users.Update;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal sealed record UpdateUserForOrganizationRequest(
    [property: Description(
        "The agency the account belongs to; an account of another agency is not found."
    )]
        Guid OrganizationId,
    [property: Description("Sign-in e-mail address. Unique within the agency.")] string Email,
    [property: Description("First name.")] string FirstName,
    [property: Description("Last name.")] string LastName,
    [property: Description("Optional job title. Omitted or null clears it.")]
        string? JobTitle = null,
    [property: Description("Optional phone number. Omitted or null clears it.")]
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
