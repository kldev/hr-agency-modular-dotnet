using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityCreatedHandler
{
    public static async Task Handle(
        SendOpportunityCreated message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendOpportunityCreated> logger,
        CancellationToken ct
    )
    {
        if (!await processedEvents.TryMarkAsync(message, ct))
        {
            logger.LogInformation("Event {EventId} already handled, skipping", message.EventId);
            return;
        }

        var html = await templates.RenderSendOpportunityCreated(message);

        await sender.SendAsync(
            new EmailMessage(
                message.ResponsiblePersonEmail,
                message.ResponsiblePersonFullName,
                $"New opportunity: {message.OpportunityTitle}",
                html
            ),
            ct
        );
    }
}
