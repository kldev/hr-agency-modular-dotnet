using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.Extensions.Logging;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Candidates.UpdateApplication;

// ReSharper disable once UnusedType.Global
public static class UpdateApplicationHandler
{
    [AggregateHandler]
    public static async Task<(CandidateApplicationUpdated, Wolverine.Marten.Events)> 
        Handle(UpdateCandidateApplication command, 
            Domain.Candidates.Candidate aggregate,
            IRecruitmentService service,
            ILogger logger,
            IClock clock,
        CancellationToken ct)
    {
        logger.HandlingUpdateApplication(command.CompanyId);
        var company = await service.GetCompanyAsync(command.CompanyId, ct);
        
        var @event = new 
            CandidateApplicationUpdated(
                aggregate.Id.Value, 
                command.JobPostId,
                company.Id, 
                clock.UtcNow);
        
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