using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.Extensions.Logging;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Candidate.UpdateApplication;

// ReSharper disable once UnusedType.Global
public static class UpdateApplicationHandler
{
    [AggregateHandler]
    public static async Task<(CandidateApplicationUpdated, Wolverine.Marten.Events)> 
        Handle(UpdateApplication command, Domain.Candidates.Candidate aggregate,
            ICompanySnapshotRepository snapshotRepository,
            ILogger logger,
        CancellationToken ct)
    {
        logger.HandlingUpdateApplication(command.CompanyId);
        var company = await snapshotRepository.GetCompanyAsync(command.CompanyId, ct);
        if (company is null) throw new BusinessRuleException(ICompanySnapshotRepository.NotFoundMessage);
        var @event = new CandidateApplicationUpdated(aggregate.Id.Value, command.JobPostId, command.CompanyId);
        
        return (@event, [@event]);
    }
}

internal static partial class CandidateLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Handling UpdateApplication event {companyId}")]
    public static partial void HandlingUpdateApplication(
        this ILogger logger,
        Guid companyId);
}