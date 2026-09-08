using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Candidates.Create;

internal static class CandidateDataFactory
{
    internal static (CandidateData, List<string> errors) Create(ICandidateData data, bool skipValidation = false)
    {
        var (phone, phoneError) = CandidatePhoneNumber.TryCreate(data.Phone);
        var (firstName, _) = FirstName.TryCreate(data.FirstName ?? "", false);
        var (lastName, _) = LastName.TryCreate(data.LastName ?? "", false);
        var (note, _) = LongText.TryCreate(data.Note ?? "", false);

        var errors = new List<string>();
        if (phoneError != null) errors.Add(phoneError);
        
        return (new CandidateData(phone!, firstName!, lastName!, note!), errors);
    }
    

    internal record CandidateData(
        CandidatePhoneNumber Phone,
        FirstName FirstName,
        LastName LastName,
        LongText Note);
}