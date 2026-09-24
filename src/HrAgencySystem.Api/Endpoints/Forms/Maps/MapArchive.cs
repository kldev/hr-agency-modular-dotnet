using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.FormDefinitions.Archive;
using HrAgencySystem.Forms.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapArchive
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.Forms.Archive, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Archive a form, closing it to new responses")
            .WithName("Archive form")
            .ProducesStandardErrors()
            .Produces<FormArchived>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid formId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormArchived>(new ArchiveForm(formId, user.OrganizationId, user.UserId), ct)
        );
}
