using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityCreatedHandler
{
    public static Task Handle(
        SendOpportunityCreated message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        NotificationMetrics metrics,
        ILogger<SendOpportunityCreated> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            metrics,
            async () =>
            {
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
            },
            ct
        );
}
