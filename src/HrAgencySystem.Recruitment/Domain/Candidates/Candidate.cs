using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Domain.Candidates;

public sealed class Candidate
{
    private Candidate(){}
    
    public CandidateId Id { get; private set; }
    public Email Email { get; private set; } = null!;
    public CandidatePhoneNumber PhoneNumber { get; private set; } = null!;
    public CandidateSource Source { get; private set; }
    public CandidateStatus Status { get; private set; }
    
    public FirstName  FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    
    public static Candidate Empty()
    {
        return new Candidate();
    }

    public void Apply(CandidateCreated @event)
    {
        Id = CandidateId.From(@event.CandidateId);
        Email = Email.Create(@event.Email);
        Source = @event.Source;
        Status = CandidateStatus.Active;
        PhoneNumber = CandidatePhoneNumber.Create(@event.Phone);
        FirstName = FirstName.Create(@event.FirstName, false);
        LastName = LastName.Create(@event.LastName, false);
    }
}