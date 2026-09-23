using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.FormDefinitions.Publish;
using HrAgencySystem.Forms.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapPublish
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.Forms.Publish, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Publish the working draft as the next version")
            .WithName("Publish form")
            .ProducesStandardErrors()
            .Produces<FormPublished>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid formId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormPublished>(new PublishForm(formId, user.OrganizationId, user.UserId), ct)
        );
}
