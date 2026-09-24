using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/form-responses/{responseId} - the answers together with the frozen version they
        // belong to, and the history of the response.
        endpoints
            .MapGet(ApiEndpoints.FormResponses.Get, Handler)
            .WithSummary("Get a form response with its version and history")
            .WithName("Get form response")
            .ProducesStandardErrors()
            .Produces<FormResponseView>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormResponsesQueryRepository repository,
        [FromRoute] Guid responseId,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await repository.GetResponse(user.GetOrganization, responseId, ct)
                ?? throw new NotFoundException("Form response", responseId)
        );
}
