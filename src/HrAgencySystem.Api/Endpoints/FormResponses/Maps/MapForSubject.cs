using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

internal static class MapForSubject
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/subjects/worker/{id}/form-responses - a worker's "Forms" tab.
        endpoints
            .MapGet(ApiEndpoints.FormResponses.ForSubject, Handler)
            .WithSummary("Get the form responses of a person")
            .WithName("Get subject form responses")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<FormResponseProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormResponsesQueryRepository repository,
        [FromRoute] string subjectKind,
        [FromRoute] Guid subjectId,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await repository.GetForSubject(user.GetOrganization, new SubjectRef(subjectKind, subjectId), ct)
        );
}
