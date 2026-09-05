using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.Extensions.Logging;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Candidate.UpdateApplication;

public static class UpdateApplicationHandler
{
    [AggregateHandler]
    public static async Task<(CandidateApplicationUpdated, Wolverine.Marten.Events)> 
        Handle(UpdateApplication command, Domain.Candidates.Candidate aggregate,
            ICompanySnapshotRepository snapshotRepository,
            ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation($"Handling UpdateApplication event {command.CompanyId} ");
        var @event = new CandidateApplicationUpdated(command.CandidateId, command.JobPostId, command.CompanyId);
        await Task.Delay(0, ct);
        
        return (@event, [@event]);
    }
}