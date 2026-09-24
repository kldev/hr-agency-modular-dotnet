using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain.Layout;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapPreviewLayout
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/forms/{formId}/preview-layout - writes nothing. Resolves system fields of a
        // layout that may not even be saved, so the preview renders exactly what publishing would.
        endpoints
            .MapPost(ApiEndpoints.Forms.PreviewLayout, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Resolve a layout as it would be published, without saving it")
            .WithName("Preview form layout")
            .ProducesStandardErrors()
            .Produces<FormLayoutPreview>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid formId,
        PreviewLayoutRequest request,
        IFormsQueryRepository repository,
        CancellationToken ct
    )
    {
        // The form itself is checked so the route cannot be used on another organization's id.
        _ =
            await repository.GetForm(user.GetOrganization, formId, ct)
            ?? throw new SharedKernel.Exception.NotFoundException("Form", formId);

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        return TypedResults.Ok(
            await repository.PreviewLayout(user.GetOrganization, request.Pages ?? [], ct)
        );
    }

    internal sealed record PreviewLayoutRequest(IReadOnlyList<FormPage> Pages);
}
