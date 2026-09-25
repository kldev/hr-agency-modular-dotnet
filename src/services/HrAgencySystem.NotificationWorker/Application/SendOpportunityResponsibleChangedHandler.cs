using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityResponsibleChangedHandler
{
    public static Task Handle(
        SendOpportunityResponsibleChanged message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        NotificationMetrics metrics,
        ILogger<SendOpportunityResponsibleChanged> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            metrics,
            async () =>
            {
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
            },
            ct
        );
}
