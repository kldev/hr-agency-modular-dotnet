using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Responses.Submit;
using HrAgencySystem.Forms.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

internal static class MapSubmit
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.FormResponses.Submit, Handler)
            .WithSummary("Submit a response, making it a document")
            .WithName("Submit form response")
            .ProducesStandardErrors()
            .Produces<FormResponseSubmitted>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid responseId,
        AnswersRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormResponseSubmitted>(
                new SubmitFormResponse(responseId, user.OrganizationId, request.Answers ?? [], user.UserId),
                ct
            )
        );
}
