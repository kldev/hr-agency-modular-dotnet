namespace HrAgencySystem.SharedKernel.Exception;

/// <summary>
/// A <see cref="ValidationException"/> that also says <em>which</em> field each message belongs to,
/// for screens with more fields than a flat list can reasonably point at - a generated form with
/// sixty of them.
/// <para>
/// A subclass, so everything that handles a validation failure today still does: the flat
/// <see cref="ValidationException.Errors"/> carries every message, and the API only adds a
/// <c>fieldErrors</c> object next to it. The key is whatever the screen names its fields by - a
/// field code in a filled form, a field or page id in the form builder.
/// </para>
/// </summary>
public sealed class FieldValidationException(
    List<string> errors,
    IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors
) : ValidationException(errors)
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; } = fieldErrors;
}
