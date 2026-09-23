using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Forms.Domain.Responses;

/// <summary>
/// What a new response starts with. For a system field: the person's profile first (what they last
/// declared in any form), then the worker's file for the few fields it answers, then nothing. For a
/// form field: its default value.
/// <para>
/// Once, at the start. A draft does not follow the profile afterwards, and a submitted response
/// never does - it is a document (plan 028 §3.7).
/// </para>
/// </summary>
public static class ResponsePrefill
{
    public static IReadOnlyList<FieldAnswer> For(
        IReadOnlyList<FormPage> pages,
        SubjectProfile? profile,
        WorkerSnapshot? worker,
        IReadOnlyList<SystemField> catalogue
    )
    {
        var answers = new List<FieldAnswer>();

        foreach (var field in pages.AllFields)
        {
            var value = field.Source == FieldSource.System
                ? profile?.ValueOf(field.Code)?.Value ?? FromWorker(field, worker, catalogue)
                : field.DefaultValue;

            if (value is not null && !value.IsEmpty && !value.HasValueOutside(FieldValue.SlotFor(field.Type)))
                answers.Add(new FieldAnswer(field.Code, value));
        }

        return answers;
    }

    /// <summary>
    /// The source is read from today's catalogue, not from the frozen version: where a value may be
    /// suggested from is a matter of pre-filling, not of what the version asks.
    /// </summary>
    private static FieldValue? FromWorker(FormField field, WorkerSnapshot? worker, IReadOnlyList<SystemField> catalogue)
    {
        if (worker is null)
            return null;

        var source = catalogue.FirstOrDefault(definition => definition.SystemFieldId == field.SystemFieldId)?.Source;

        return SourceValue(source ?? SystemFieldSource.None, worker);
    }

    public static FieldValue? SourceValue(SystemFieldSource source, WorkerSnapshot worker) =>
        source switch
        {
            SystemFieldSource.WorkerFirstName => FieldValue.OfText(worker.FirstName),
            SystemFieldSource.WorkerLastName => FieldValue.OfText(worker.LastName),
            SystemFieldSource.WorkerDateOfBirth => FieldValue.OfDate(worker.DateOfBirth),
            SystemFieldSource.WorkerCitizenship => FieldValue.OfText(worker.Citizenship),
            SystemFieldSource.WorkerEmail when worker.Email is not null => FieldValue.OfText(worker.Email),
            SystemFieldSource.WorkerPhone when worker.Phone is not null => FieldValue.OfText(worker.Phone),
            _ => null,
        };
}
