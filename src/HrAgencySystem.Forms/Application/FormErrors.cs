using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Forms.Application;

/// <summary>
/// Turns the domain's error lists into the one exception the API knows how to show field by field.
/// The flat list names each field by its label, so a client that ignores <c>fieldErrors</c> still
/// reads something useful.
/// </summary>
internal static class FormErrors
{
    /// <summary>Layout errors, keyed by the page or field id the builder marks; form-wide ones under "".</summary>
    public static FieldValidationException From(IReadOnlyList<LayoutError> errors) =>
        new(
            [.. errors.Select(error => error.ToMessage()).Distinct()],
            errors
                .GroupBy(error => error.Target?.ToString() ?? "")
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<string>)[.. group.Select(error => error.Message).Distinct()]
                )
        );

    /// <summary>Answer errors, keyed by field code - what the filled form names its fields by.</summary>
    public static FieldValidationException From(
        IReadOnlyList<FieldError> errors,
        IReadOnlyDictionary<string, string> labels
    ) =>
        new(
            [.. errors.Select(error => $"{labels.GetValueOrDefault(error.FieldCode, error.FieldCode)}: {error.Message}")],
            errors
                .GroupBy(error => error.FieldCode)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<string>)[.. group.Select(error => error.Message)]
                )
        );
}
