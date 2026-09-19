using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityResponsibleChangedHandler
{
    public static Task Handle(
        SendOpportunityResponsibleChanged message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendOpportunityResponsibleChanged> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
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
