using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobApplicationCreatedHandler
{
    public static async Task Handle(
        SendJobApplicationCreated message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendJobApplicationCreated> logger,
        CancellationToken ct
    )
    {
        if (!await processedEvents.TryMarkAsync(message, ct))
        {
            logger.LogInformation("Event {EventId} already handled, skipping", message.EventId);
            return;
        }

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
    }
}
