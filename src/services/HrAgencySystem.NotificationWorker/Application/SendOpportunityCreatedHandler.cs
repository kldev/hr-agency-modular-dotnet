using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityCreatedHandler
{
    public static async Task Handle(
        SendOpportunityCreated message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        CancellationToken ct
    )
    {
        var html = await templates.RenderSendOpportunityCreated(message);

        await sender.SendAsync(
            new EmailMessage(
                message.ResponsiblePersonEmail,
                message.ResponsiblePersonFullName,
                $"New opportunity: {message.OpportunityTitle}",
                html
            ),
            ct
        );
    }
}
