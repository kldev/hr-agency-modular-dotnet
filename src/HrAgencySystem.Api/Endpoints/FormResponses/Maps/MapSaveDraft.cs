using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Responses.SaveDraft;
using HrAgencySystem.Forms.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

internal static class MapSaveDraft
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPut(ApiEndpoints.FormResponses.Draft, Handler)
            .WithSummary("Save the answers of an unfinished response")
            .WithName("Save form response draft")
            .ProducesStandardErrors()
            .Produces<FormResponseDraftSaved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid responseId,
        AnswersRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormResponseDraftSaved>(
                new SaveFormResponseDraft(responseId, user.OrganizationId, request.Answers ?? [], user.UserId),
                ct
            )
        );
}
