using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobApplicationCreatedHandler
{
    public static async Task Handle(
        SendJobApplicationCreated message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        CancellationToken ct
    )
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
    }
}
