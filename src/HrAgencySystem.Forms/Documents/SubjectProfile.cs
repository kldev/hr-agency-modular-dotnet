using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Values;

namespace HrAgencySystem.Forms.Documents;

/// <summary>
/// The last known value of each system field for one person - "one value, many forms". A
/// document, not a stream: it is a set of latest values, and their history already lives in the
/// responses they came from (<see cref="ProfileValue.ResponseId"/>).
/// <para>
/// Written when a response is submitted or corrected, in the same transaction. Read when a response
/// is started, to pre-fill it. Never written back to the worker's file.
/// </para>
/// </summary>
public sealed record SubjectProfile(
    Guid Id,
    Guid OrganizationId,
    string SubjectKind,
    Guid SubjectId,
    IReadOnlyList<ProfileValue> Values
)
{
    public static SubjectProfile EmptyFor(Guid organizationId, SubjectRef subject) =>
        new(FormsStreamId.ForProfile(organizationId, subject), organizationId, subject.Kind, subject.Id, []);

    public ProfileValue? ValueOf(string code) => Values.FirstOrDefault(value => value.Code == code);

    /// <summary>
    /// Takes the given answers as the latest values, unless the profile already holds something
    /// newer for that code - correcting last year's statement must not overwrite the phone number
    /// somebody gave last week in another form.
    /// </summary>
    public SubjectProfile With(
        IEnumerable<FieldAnswer> answers,
        Guid responseId,
        DateTimeOffset answeredAt
    )
    {
        var values = Values.ToDictionary(value => value.Code);

        foreach (var answer in answers)
        {
            if (values.TryGetValue(answer.FieldCode, out var current) && current.UpdatedAt > answeredAt)
                continue;

            values[answer.FieldCode] = new ProfileValue(answer.FieldCode, answer.Value, responseId, answeredAt);
        }

        return this with { Values = [.. values.Values.OrderBy(value => value.Code, StringComparer.Ordinal)] };
    }
}

public sealed record ProfileValue(string Code, FieldValue Value, Guid ResponseId, DateTimeOffset UpdatedAt);
