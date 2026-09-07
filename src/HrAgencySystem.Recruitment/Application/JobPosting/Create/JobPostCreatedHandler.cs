using HrAgencySystem.Recruitment.Contracts.IntegrationEvents;
using HrAgencySystem.Recruitment.Events.JobPostings;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace HrAgencySystem.Recruitment.Application.JobPosting.Create;

[WolverineHandler]
public class JobPostCreatedHandler
{
    public async Task<JobPostCreatedIntegrationEvent> HandleAsync(JobPostCreated message,  ILogger<JobPostCreated> logger)
    {
        logger.LogInformation("Handling JobPostCreated event");
        await Task.Delay(100);
        var result = new JobPostCreatedIntegrationEvent(message.CompanyId, message.JobPostId);
        return result;
    }
}