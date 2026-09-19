using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendPasswordResetHandler
{
    public static Task Handle(
        SendPasswordReset message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendPasswordReset> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            async () =>
            {
                var html = await templates.RenderSendPasswordReset(message);

                await sender.SendAsync(
                    new EmailMessage(
                        message.RecipientEmail,
                        message.RecipientFullname,
                        "Reset your password",
                        html
                    ),
                    ct
                );
            },
            ct
        );
}
