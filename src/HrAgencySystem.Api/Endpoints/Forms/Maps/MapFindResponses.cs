using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapFindResponses
{
    public const string FieldRequiredMessage = "Name the field to search by.";
    public const string ValueRequiredMessage = "Give exactly one value to search for.";

    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/forms/{formId}/responses?field=gdpr.consent&boolean=true - "who answered X".
        // The proof that answers can be asked, not a report: that is reports.form_answers, later.
        endpoints
            .MapGet(ApiEndpoints.Forms.Responses, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Find responses to a form by the value of one field")
            .WithName("Find form responses")
            .ProducesStandardErrors()
            .Produces<SliceResponse<FormResponseProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IFormResponsesQueryRepository repository,
        [FromRoute] Guid formId,
        [FromQuery] string? field,
        [FromQuery] string? text,
        [FromQuery] decimal? number,
        [FromQuery] DateOnly? date,
        [FromQuery] bool? boolean,
        [FromQuery] string[]? values,
        [FromQuery] FormResponseStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ValidationException(FieldRequiredMessage);

        var value = new FieldValue(text, number, date, boolean, values is { Length: > 0 } ? values : null);
        var given = new object?[] { value.Text, value.Number, value.Date, value.Boolean, value.Values };

        if (given.Count(v => v is not null) != 1)
            throw new ValidationException(ValueRequiredMessage);

        return TypedResults.Ok(
            await repository.FindByAnswer(
                user.GetOrganization,
                formId,
                new FormAnswerQuery(field.Trim(), value, status, page, pageSize),
                ct
            )
        );
    }
}
