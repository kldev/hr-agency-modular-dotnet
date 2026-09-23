using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/forms - open to every member: the people who fill forms in pick from this list.
        endpoints
            .MapGet(ApiEndpoints.Forms.Slice, Handler)
            .WithSummary("Get a slice of forms")
            .WithName("Get forms")
            .ProducesStandardErrors()
            .Produces<SliceResponse<FormDefinitionProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormsQueryRepository repository,
        [FromQuery] string? search,
        [FromQuery] FormStatus[]? status,
        [FromQuery] FormKind[]? kind,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    ) =>
        TypedResults.Ok(
            await repository.GetForms(
                user.GetOrganization,
                new FormQuery(search ?? "", status, kind, page, pageSize),
                ct
            )
        );
}
