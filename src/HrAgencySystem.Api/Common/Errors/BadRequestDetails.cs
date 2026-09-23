using System.Text.Json.Serialization;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Common.Errors;

public sealed class BadRequestDetails : ProblemDetails
{
    [JsonConstructor]
    public BadRequestDetails(IReadOnlyCollection<string> validationErrors)
    {
        ValidationErrors = validationErrors;
        Status = StatusCodes.Status400BadRequest;
    }

    public IReadOnlyCollection<string> ValidationErrors { get; }

    /// <summary>
    /// The same messages, keyed by the field they belong to - only for screens that name their
    /// fields (a generated form, the form builder). Absent everywhere else, so no existing client
    /// sees a change.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? FieldErrors { get; init; }

    public static BadRequestDetails CreateValidation(ValidationException exception)
    {
        return new BadRequestDetails(exception.Errors)
        {
            Detail = "Request contains invalid fields",
            FieldErrors = exception is FieldValidationException { FieldErrors.Count: > 0 } fields
                ? fields.FieldErrors
                : null,
        };
    }

    public static implicit operator ProblemHttpResult(BadRequestDetails details)
    {
        return TypedResults.Problem(
            statusCode: details.Status ?? StatusCodes.Status400BadRequest,
            detail: details.Detail,
            extensions: GetExtensions(details)
        );
    }

    private static Dictionary<string, object?> GetExtensions(BadRequestDetails details)
    {
        var extensions = new Dictionary<string, object?>();

        if (details.ValidationErrors is { Count: > 0 })
            extensions["validationErrors"] = details.ValidationErrors;

        if (details.FieldErrors is { Count: > 0 })
            extensions["fieldErrors"] = details.FieldErrors;

        return extensions;
    }
}
