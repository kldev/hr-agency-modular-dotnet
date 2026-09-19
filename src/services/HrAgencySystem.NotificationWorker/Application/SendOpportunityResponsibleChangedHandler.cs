using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityResponsibleChangedHandler
{
    public static async Task Handle(
        SendOpportunityResponsibleChanged message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendOpportunityResponsibleChanged> logger,
        CancellationToken ct
    )
    {
        if (!await processedEvents.TryMarkAsync(message, ct))
        {
            logger.LogInformation("Event {EventId} already handled, skipping", message.EventId);
            return;
        }

        var html = await templates.RenderSendOpportunityResponsibleChanged(message);

        await sender.SendAsync(
            new EmailMessage(
                message.ResponsibleEmail,
                message.ResponsibleFullname,
                $"Opportunity assigned to you: {message.OpportunityTitle}",
                html
            ),
            ct
        );
    }
}
