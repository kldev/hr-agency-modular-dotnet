using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobApplicationCreatedHandler
{
    public static Task Handle(
        SendJobApplicationCreated message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        NotificationMetrics metrics,
        ILogger<SendJobApplicationCreated> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            metrics,
            async () =>
            {
                var html = await templates.RenderSendJobApplicationCreated(message);

                await sender.SendAsync(
                    new EmailMessage(
                        message.RecruiterEmail,
                        message.RecruiterFullname,
                        $"New application: {message.ApplicantFullname} for {message.JobPostTitle}",
                        html
                    ),
                    ct
                );
            },
            ct
        );
}
