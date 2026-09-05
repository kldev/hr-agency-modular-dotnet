using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.Candidate.UpdateApplication;
using HrAgencySystem.Recruitment.Application.JobApplication.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Domain.Candidate.ValueObjects;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace HrAgencySystem.Recruitment.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CandidateResolver(IMessageBus bus, IDocumentSession session, ILogger<ICandidateResolver> logger): ICandidateResolver
{
    public async Task<CandidateInfo> FindOrCreate(CreateCandidate command, JobPostInfo? info, CancellationToken ct)
    {
        var existing = await GetExisting(command, ct);
        if (existing is null) return await CreateNew(command, ct);
        if (info != null)
        {
            logger.LogInformation($"Candidate {command.Email} already in database. Update");
            await bus.InvokeAsync<CandidateApplicationUpdated>(
                new UpdateApplication(existing.CandidateId, info.Id, info.CompanyId, command.Source), ct);
        }

        return existing!;

    }

    private async Task<CandidateInfo> CreateNew(CreateCandidate command, CancellationToken ct)
    {
        logger.LogInformation($"Candidate {command.Email} not in database. Create new");
        var result = await bus.InvokeAsync<CandidateCreated>(command, ct);

        logger.LogInformation($"Candidate {command.Email} created successfully");
        
        return new CandidateInfo(result.Id, result.Email,
            result.PhoneNumber, result.FirstName, result.LastName);
    }

    private async Task<CandidateInfo?> GetExisting(CreateCandidate command, CancellationToken ct)
    {
        var email = Email.Create(command.Email);
        var projection = await session.Query<CandidateProjection>()
            .Where(z => z.OrgId == command.OrganizationId && z.Email == email.Value).FirstOrDefaultAsync(ct);

        return projection != null
            ? new CandidateInfo(projection.Id, email.Value, projection.PhoneNumber,
                projection.FirstName ?? "", projection.LastName ?? "")
            : null;
    }
}

