namespace HrAgencySystem.Recruitment.Application.Candidates.Create;

internal interface ICandidateData
{
    string Phone { get; }
    string FirstName { get; }
    string LastName { get; }
    string Note { get; }
}