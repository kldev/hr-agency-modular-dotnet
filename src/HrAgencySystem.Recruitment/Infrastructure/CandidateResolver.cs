using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Events.Candidate;
using Marten;
using Wolverine;

namespace HrAgencySystem.Recruitment.Infrastructure;

public sealed class CandidateResolver(IMessageBus bus, IDocumentSession session): ICandidateResolver
{
    public Candidate FindOrCreate(CreateCandidate command)
    {
        var candidate = Candidate.Empty();

        candidate.Apply(new CandidateCreated(Guid.NewGuid(), command.Email, command.PhoneNumber, command.Source));
        // TODO: handler / read
        
        return candidate;
    }
}