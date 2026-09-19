using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityResponsibleChangedHandler
{
    public static async Task Handle(
        SendOpportunityResponsibleChanged message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        CancellationToken ct
    )
    {
        var html = await templates.RenderSendOpportunityResponsibleChanged(message);

        await sender.SendAsync(
            new EmailMessage(
                message.ResponsibleEmail,
                message.ResponsibleFullname,
                $"Opportunity assigned to you: {message.OpportunityTitle}",
                html
            ),
            ct
        );
    }
}
