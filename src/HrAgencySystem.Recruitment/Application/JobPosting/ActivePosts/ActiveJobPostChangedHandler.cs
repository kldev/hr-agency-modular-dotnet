using HrAgencySystem.Recruitment.Contracts.IntegrationEvents;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace HrAgencySystem.Recruitment.Application.JobPosting.ActivePosts;

[WolverineHandler]
public class ActiveJobPostChangedHandler
{
    public async Task<JobPostActiveChangedIntegrationEvent?> HandleAsync(JobPostStatusChanged message, ILogger<JobPostStatusChanged> logger)
    {
        logger.LogInformation("Handling JobPostStatusChanged event");
        await Task.Delay(100);
        
        var delta = (message.OldStatus, message.NewStatus) switch
        {
            (_, JobPostStatus.Published) => +1,
            (JobPostStatus.Published, _) => -1,
            _ => 0
        };
        return delta == 0 ? null : new JobPostActiveChangedIntegrationEvent(message.JobPostId, message.CompanyId, delta);
    }
}
