using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.SystemFields.AddStandard;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SystemFields.Maps;

internal static class MapAddStandard
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.SystemFields.Standard, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Add the standard system fields the catalogue does not have yet")
            .WithName("Add standard system fields")
            .ProducesStandardErrors()
            .Produces<StandardSystemFieldsAdded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<StandardSystemFieldsAdded>(
                new AddStandardSystemFields(user.OrganizationId, user.UserId),
                ct
            )
        );
}
