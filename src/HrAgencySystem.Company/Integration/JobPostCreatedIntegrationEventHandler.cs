using HrAgencySystem.Company.Events;
using HrAgencySystem.Recruitment.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace HrAgencySystem.Company.Integration;

[WolverineHandler]
public class JobPostCreatedIntegrationEventHandler
{
    public async Task HandleAsync(JobPostCreatedIntegrationEvent message, IMessageBus bus,
        ILogger<JobPostCreatedIntegrationEvent> logger)
    {
        logger.LogInformation("Handling JobPostCreatedIntegrationEvent  event");
        var @event = new CompanyJobPostCreated(message.CompanyId, message.JobPostId);
        await bus.InvokeAsync(@event);
    }
}

public static class JobPostCreatedHandler
{
    [AggregateHandler]
    public static Task<CompanyJobPostCreated> Handle(CompanyJobPostCreated command, Domain.Company aggregate)
    {
        return Task.FromResult(command);
    }
}