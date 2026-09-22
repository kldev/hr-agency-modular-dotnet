using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendTimeSheetReturnedForCorrectionHandler
{
    public static Task Handle(
        SendTimeSheetReturnedForCorrection message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendTimeSheetReturnedForCorrection> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            async () =>
            {
                var html = await templates.RenderSendTimeSheetReturnedForCorrection(message);

                await sender.SendAsync(
                    new EmailMessage(
                        message.RecipientEmail,
                        message.RecipientFullname,
                        $"Hours for {message.Period} sent back for correction",
                        html
                    ),
                    ct
                );
            },
            ct
        );
}
