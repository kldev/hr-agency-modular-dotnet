using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendTeamMemberRoleChangedHandler
{
    public static Task Handle(
        SendTeamMemberRoleChanged message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendTeamMemberRoleChanged> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            async () =>
            {
                var html = await templates.RenderSendTeamMemberRoleChanged(message);

                await sender.SendAsync(
                    new EmailMessage(
                        message.MemberEmail,
                        message.MemberFullname,
                        $"Your role on {message.TeamName} changed",
                        html
                    ),
                    ct
                );
            },
            ct
        );
}
