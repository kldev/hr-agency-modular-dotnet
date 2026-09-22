using HrAgencySystem.Recruitment.Application.Candidates.Create;
using HrAgencySystem.Recruitment.Application.Candidates.UpdateApplication;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace HrAgencySystem.Recruitment.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CandidateResolver(
    IMessageBus bus,
    IDocumentSession session,
    ILogger<ICandidateResolver> logger
) : ICandidateResolver
{
    public async Task<CandidateInfo> FindOrCreate(
        CreateCandidate command,
        JobPostInfo info,
        CancellationToken ct
    )
    {
        var existing = await GetExisting(command, ct);
        if (existing is null)
            return await CreateNew(command, ct);

        logger.CandidateFoundInDatabase(existing.CandidateId);
        await bus.InvokeAsync<CandidateApplicationUpdated>(
            new UpdateCandidateApplication(
                existing.CandidateId,
                info.Id,
                info.CompanyId,
                command.Source
            ),
            ct
        );

        return existing;
    }

    private async Task<CandidateInfo> CreateNew(CreateCandidate command, CancellationToken ct)
    {
        logger.CandidateNotInDatabase(command.OrganizationId);
        var result = await bus.InvokeAsync<CandidateCreated>(command, ct);

        logger.CandidateCreatedSuccessfully(result.CandidateId);

        return new CandidateInfo(
            result.CandidateId,
            result.Email,
            result.Phone,
            result.FirstName,
            result.LastName
        );
    }

    private async Task<CandidateInfo?> GetExisting(CreateCandidate command, CancellationToken ct)
    {
        var email = Email.Create(command.Email);
        var projection = await session
            .Query<CandidateProjection>()
            .Where(z => z.OrgId == command.OrganizationId && z.Email == email.Value)
            .Select(z => new CandidateInfo(z.Id, z.Email, z.PhoneNumber, z.FirstName, z.LastName))
            .FirstOrDefaultAsync(ct);

        if (projection != null)
        {
            return projection;
        }

        var @event = await session
            .Query<CandidateCreated>()
            .Where(z => z.Email == email.Value && z.OrganizationId == command.OrganizationId)
            .Select(z => new CandidateInfo(
                z.CandidateId,
                z.Email,
                z.Phone,
                z.FirstName,
                z.LastName
            ))
            .FirstOrDefaultAsync(ct);

        if (@event != null)
            logger.CandidateFoundInEvents(@event.CandidateId);

        return @event;
    }
}

// Identifiers, not people: a candidate's e-mail is personal data and stays out of the log.
internal static partial class CandidateLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Candidate {CandidateId} created successfully"
    )]
    public static partial void CandidateCreatedSuccessfully(this ILogger logger, Guid candidateId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Candidate not in organization {OrganizationId}. Create new"
    )]
    public static partial void CandidateNotInDatabase(this ILogger logger, Guid organizationId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Candidate {CandidateId} found in database"
    )]
    public static partial void CandidateFoundInDatabase(this ILogger logger, Guid candidateId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Candidate {CandidateId} found in events"
    )]
    public static partial void CandidateFoundInEvents(this ILogger logger, Guid candidateId);
}
