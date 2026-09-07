using HrAgencySystem.Company.Events;
using HrAgencySystem.Recruitment.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace HrAgencySystem.Company.Integration;

[WolverineHandler]
public class JobPostActiveChangedIntegrationEventHandler
{
    public async Task HandleAsync(JobPostActiveChangedIntegrationEvent message, IMessageBus bus,
        ILogger<JobPostCreatedIntegrationEvent> logger)
    {
        logger.LogInformation("Handling JobPostActiveChangedIntegrationEvent  event");
        var @event = new CompanyJobPostActiveChanged(message.JobPostId, message.CompanyId, message.ChangeBy);
        await bus.InvokeAsync(@event);
    }
}

public static class CompanyJobPostActiveChangedHandler
{
    [AggregateHandler]
    public static Task<CompanyJobPostActiveChanged> Handle(CompanyJobPostActiveChanged command, Domain.Company aggregate)
    {
        return Task.FromResult(command);
    }
}