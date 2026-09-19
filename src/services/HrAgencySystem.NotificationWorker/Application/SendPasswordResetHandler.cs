using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendPasswordResetHandler
{
    public static async Task Handle(
        SendPasswordReset message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        CancellationToken ct
    )
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
    }
}
