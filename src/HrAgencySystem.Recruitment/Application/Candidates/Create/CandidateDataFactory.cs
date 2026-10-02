using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Candidates.Create;

internal static class CandidateDataFactory
{
    /// <summary>
    /// Every value object and every error. The data is only usable when the list is empty - each
    /// value that failed is null - so callers throw on errors before reading it.
    /// </summary>
    internal static (CandidateData, List<string> errors) Create(ICandidateData data)
    {
        var (phone, phoneError) = CandidatePhoneNumber.TryCreate(data.Phone);
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        var (firstName, firstNameError) = FirstName.TryCreate(data.FirstName ?? "", false);
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        var (lastName, lastNameError) = LastName.TryCreate(data.LastName ?? "", false);
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        var (note, noteError) = LongText.TryCreate(data.Note ?? "");

        List<string> errors =
        [
            .. new[] { phoneError, firstNameError, lastNameError, noteError }.OfType<string>(),
        ];

        return (new CandidateData(phone!, firstName!, lastName!, note!), errors);
    }

    internal record CandidateData(
        CandidatePhoneNumber Phone,
        FirstName FirstName,
        LastName LastName,
        LongText Note
    );
}
