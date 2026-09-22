using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendTimeSheetApprovedHandler
{
    public static Task Handle(
        SendTimeSheetApproved message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendTimeSheetApproved> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            async () =>
            {
                var html = await templates.RenderSendTimeSheetApproved(message);

                await sender.SendAsync(
                    new EmailMessage(
                        message.RecipientEmail,
                        message.RecipientFullname,
                        $"Hours for {message.Period} approved",
                        html
                    ),
                    ct
                );
            },
            ct
        );
}
