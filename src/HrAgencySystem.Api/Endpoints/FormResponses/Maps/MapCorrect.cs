using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Responses.Correct;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

internal static class MapCorrect
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.FormResponses.Correct, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Correct a submitted response, giving the reason")
            .WithName("Correct form response")
            .ProducesStandardErrors()
            .Produces<FormResponseCorrected>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid responseId,
        CorrectFormResponseRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormResponseCorrected>(
                new CorrectFormResponse(
                    responseId,
                    user.OrganizationId,
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    request.Answers
                        ?? [],
                    request.Reason,
                    user.UserId
                ),
                ct
            )
        );

    internal sealed record CorrectFormResponseRequest(
        IReadOnlyList<FieldAnswer> Answers,
        string Reason
    );
}
