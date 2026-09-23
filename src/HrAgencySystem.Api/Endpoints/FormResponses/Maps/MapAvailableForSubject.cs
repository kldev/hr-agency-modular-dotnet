using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

internal static class MapAvailableForSubject
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/subjects/worker/{id}/available-forms - what can be started for this person now.
        endpoints
            .MapGet(ApiEndpoints.FormResponses.AvailableForSubject, Handler)
            .WithSummary("Get the forms that can be started for a person")
            .WithName("Get subject available forms")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<FormDefinitionProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormsQueryRepository repository,
        [FromRoute] string subjectKind,
        [FromRoute] Guid subjectId,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await repository.GetAvailableForms(user.GetOrganization, new SubjectRef(subjectKind, subjectId), ct)
        );
}
