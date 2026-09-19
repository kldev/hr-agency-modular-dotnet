using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendTeamMemberAddedHandler
{
    public static Task Handle(
        SendTeamMemberAdded message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        ILogger<SendTeamMemberAdded> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            async () =>
            {
                var html = await templates.RenderSendTeamMemberAdded(message);

                await sender.SendAsync(
                    new EmailMessage(
                        message.MemberEmail,
                        message.MemberFullname,
                        $"You joined the team: {message.TeamName}",
                        html
                    ),
                    ct
                );
            },
            ct
        );
}
