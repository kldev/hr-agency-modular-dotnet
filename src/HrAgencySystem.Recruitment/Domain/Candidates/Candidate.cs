using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Domain.Candidates;

public sealed class Candidate
{
    private Candidate() { }

    public CandidateId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }
    public Email Email { get; private set; } = null!;
    public CandidatePhoneNumber PhoneNumber { get; private set; } = null!;
    public CandidateSource Source { get; private set; }
    public CandidateStatus Status { get; private set; }

    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;

    public LongText Note { get; private set; } = null!;

    /// <summary>The workers' file opened for this person, if one was. At most one - see the handler.</summary>
    public Guid? WorkerId { get; private set; }

    public static Candidate Empty()
    {
        return new Candidate();
    }

    public void Apply(CandidateCreated @event)
    {
        Id = CandidateId.From(@event.CandidateId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Email = Email.Create(@event.Email);
        Source = @event.Source;
        Status = CandidateStatus.Active;
        PhoneNumber = CandidatePhoneNumber.Create(@event.Phone);
        FirstName = FirstName.Create(@event.FirstName, false);
        LastName = LastName.Create(@event.LastName, false);
        Note = LongText.Create(@event.Note, false);
    }

    public void Apply(CandidateRegisteredAsWorker @event)
    {
        WorkerId = @event.WorkerId;
    }

    public void Apply(CandidateUpdated @event)
    {
        PhoneNumber = CandidatePhoneNumber.Create(@event.Phone);
        FirstName = FirstName.Create(@event.FirstName, false);
        LastName = LastName.Create(@event.LastName, false);
        Note = LongText.Create(@event.Note, false);
    }
}
