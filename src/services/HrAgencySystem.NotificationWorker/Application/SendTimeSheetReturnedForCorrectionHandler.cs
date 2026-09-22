using HrAgencySystem.EmailTemplates.Contracts.Agency;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendTimeSheetReturnedForCorrectionHandler
{
    public static Task Handle(
        SendTimeSheetReturnedForCorrection message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        NotificationMetrics metrics,
        ILogger<SendTimeSheetReturnedForCorrection> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            metrics,
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
