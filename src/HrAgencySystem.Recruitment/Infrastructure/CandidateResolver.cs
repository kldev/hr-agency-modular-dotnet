using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Domain.Candidate.ValueObjects;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Wolverine;

namespace HrAgencySystem.Recruitment.Infrastructure;

public sealed class CandidateResolver(IMessageBus bus, IDocumentSession session): ICandidateResolver
{
    public async Task<CandidateInfo> FindOrCreate(CreateCandidate command, CancellationToken ct)
    {
        var existing = await GetExisting(command, ct);
        if (existing is not null) return existing!;
        
        return await CreateNew(command, ct);
    }

    private async Task<CandidateInfo> CreateNew(CreateCandidate command, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<CandidateCreated>(command, ct);

        return new CandidateInfo(result.Id, Email.Create(result.Email),
            CandidatePhoneNumber.Create(result.PhoneNumber));
    }

    private async Task<CandidateInfo?> GetExisting(CreateCandidate command, CancellationToken ct)
    {
        var email = Email.Create(command.Email);
        var projection = await session.Query<CandidateProjection>()
            .Where(z => z.OrgId == command.OrganizationId && z.Email == email.Value).FirstOrDefaultAsync(ct);

        return projection != null
            ? new CandidateInfo(projection.Id, email, CandidatePhoneNumber.Create(projection.PhoneNumber))
            : null;
    }
}

