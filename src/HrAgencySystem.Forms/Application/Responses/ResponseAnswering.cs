using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Forms.Application.Responses;

/// <summary>What saving, submitting and correcting a response share: the version, and the answers checked against it.</summary>
internal static class ResponseAnswering
{
    public const string VersionMissingMessage = "The form version this response was started on cannot be found.";

    public static async Task<FormVersion> VersionOf(
        FormResponse response,
        IFormsRepository repository,
        CancellationToken ct
    ) =>
        await repository.GetVersionAsync(response.FormId, response.FormVersion, ct)
        ?? throw new BusinessRuleException(VersionMissingMessage);

    /// <summary>Normalized answers, or a field keyed validation error against the response's own version.</summary>
    public static IReadOnlyList<FieldAnswer> Check(
        FormVersion version,
        IReadOnlyList<FieldAnswer>? answers,
        ValidationMode mode
    )
    {
        var normalized = FieldAnswers.Normalize(version.Pages, answers);
        var errors = FormAnswersValidator.Validate(version.Pages, normalized, mode);

        if (errors.Count > 0)
            throw FormErrors.From(
                errors,
                version.Pages.AllFields.ToDictionary(field => field.Code, field => field.Label)
            );

        return normalized;
    }

    /// <summary>
    /// The system field answers become the person's latest values. Written in the same transaction
    /// as the submission or correction, so a second form started a second later already sees them.
    /// </summary>
    public static async Task UpdateProfile(
        FormResponse response,
        FormVersion version,
        IReadOnlyList<FieldAnswer> answers,
        DateTimeOffset answeredAt,
        IFormsRepository repository,
        CancellationToken ct
    )
    {
        var systemCodes = version.Pages.AllFields
            .Where(field => field.Source == FieldSource.System)
            .Select(field => field.Code)
            .ToHashSet();

        var systemAnswers = answers.Where(answer => systemCodes.Contains(answer.FieldCode)).ToList();

        if (systemAnswers.Count == 0)
            return;

        var organizationId = response.OrganizationId.Value;
        var profile =
            await repository.GetProfileAsync(organizationId, response.Subject, ct)
            ?? SubjectProfile.EmptyFor(organizationId, response.Subject);

        repository.StoreProfile(profile.With(systemAnswers, response.Id, answeredAt));
    }
}
