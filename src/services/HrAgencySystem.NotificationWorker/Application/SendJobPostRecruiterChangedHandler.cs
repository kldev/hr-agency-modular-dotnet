using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobPostRecruiterChangedHandler
{
    public static async Task Handle(
        SendJobPostRecruiterChanged message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        CancellationToken ct
    )
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
    }
}
