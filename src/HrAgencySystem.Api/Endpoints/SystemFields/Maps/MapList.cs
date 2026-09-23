using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain.SystemFields;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.SystemFields.Maps;

internal static class MapList
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/system-fields - the whole catalogue; it is tens of fields, there is nothing to page.
        endpoints
            .MapGet(ApiEndpoints.SystemFields.List, Handler)
            .WithSummary("Get the catalogue of system fields")
            .WithName("Get system fields")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<SystemField>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormsQueryRepository repository,
        [FromQuery] bool includeArchived = false,
        CancellationToken ct = default
    ) => TypedResults.Ok(await repository.GetSystemFields(user.GetOrganization, includeArchived, ct));
}
