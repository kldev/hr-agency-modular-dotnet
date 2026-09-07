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
    public async Task<CandidateInfo> FindOrCreate(CreateCandidate command, JobPostInfo info, CancellationToken ct)
    {
        var existing = await GetExisting(command, ct);
        if (existing is null) return await CreateNew(command, ct);

        logger.CandidateFoundInDatabase(command.Email);
        await bus.InvokeAsync<CandidateApplicationUpdated>(
            new UpdateApplication(existing.CandidateId, info.Id, info.CompanyId, command.Source), ct);

        return existing;
    }

    private async Task<CandidateInfo> CreateNew(CreateCandidate command, CancellationToken ct)
    {
        logger.CandidateNotInDatabase(command.Email);
        var result = await bus.InvokeAsync<CandidateCreated>(command, ct);

        logger.CandidateCreatedSuccessfully(result.Email);

        return new CandidateInfo(result.CandidateId, result.Email,
            result.Phone, result.FirstName, result.LastName);
    }

    private async Task<CandidateInfo?> GetExisting(CreateCandidate command, CancellationToken ct)
    {
        var email = Email.Create(command.Email);
        var projection = await session.Query<CandidateProjection>()
            .Where(z => z.OrgId == command.OrganizationId && z.Email == email.Value).FirstOrDefaultAsync(ct);

        if (projection != null)
        {
            return new CandidateInfo(projection.Id, email.Value, projection.PhoneNumber,
                projection.FirstName ?? "", projection.LastName ?? "");
        }

        var @event = await session.Query<CandidateCreated>()
            .Where(z => z.Email == email.Value && z.OrganizationId == command.OrganizationId).FirstOrDefaultAsync(ct);

        return @event != null
            ? new CandidateInfo(@event.CandidateId, @event.Email, @event.Phone, @event.FirstName,
                @event.LastName)
            : null;
    }
}

internal static partial class CandidateLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Candidate {email} created successfully")]
    public static partial void CandidateCreatedSuccessfully(
        this ILogger logger,
        string email);
    
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Candidate {email} not in database. Create new")]
    public static partial void CandidateNotInDatabase(
        this ILogger logger,
        string email);
    
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Candidate {email} found database. ")]
    public static partial void CandidateFoundInDatabase(
        this ILogger logger,
        string email);
}

