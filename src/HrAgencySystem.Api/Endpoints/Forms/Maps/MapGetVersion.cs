using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapGetVersion
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/forms/{formId}/versions/{version} - a frozen layout, for anybody who fills forms.
        endpoints
            .MapGet(ApiEndpoints.Forms.Version, Handler)
            .WithSummary("Get a published version of a form")
            .WithName("Get form version")
            .ProducesStandardErrors()
            .Produces<FormVersion>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormsQueryRepository repository,
        [FromRoute] Guid formId,
        [FromRoute] int version,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await repository.GetVersion(user.GetOrganization, formId, version, ct)
                ?? throw new NotFoundException("Form version", $"{formId}/{version}")
        );
}
