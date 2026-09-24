using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/forms/{formId} - the builder's view: the working draft and the versions out.
        endpoints
            .MapGet(ApiEndpoints.Forms.Get, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Get a form with its working draft")
            .WithName("Get form")
            .ProducesStandardErrors()
            .Produces<FormDefinitionView>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormsQueryRepository repository,
        [FromRoute] Guid formId,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await repository.GetForm(user.GetOrganization, formId, ct)
                ?? throw new NotFoundException("Form", formId)
        );
}
