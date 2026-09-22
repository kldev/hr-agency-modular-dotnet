using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobPostRecruiterChangedHandler
{
    public static Task Handle(
        SendJobPostRecruiterChanged message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        NotificationMetrics metrics,
        ILogger<SendJobPostRecruiterChanged> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            metrics,
            async () =>
            {
                var html = await templates.RenderSendJobPostRecruiterChanged(message);

                await sender.SendAsync(
                    new EmailMessage(
                        message.RecruiterEmail,
                        message.RecruiterFullname,
                        $"Job post assigned to you: {message.JobPostTitle}",
                        html
                    ),
                    ct
                );
            },
            ct
        );
}
