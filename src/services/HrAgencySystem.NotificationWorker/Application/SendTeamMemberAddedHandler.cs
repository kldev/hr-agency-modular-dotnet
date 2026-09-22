using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.EmailTemplates.Rendering;
using HrAgencySystem.EmailTemplates.Sending;
using HrAgencySystem.NotificationWorker.Infrastructure;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendTeamMemberAddedHandler
{
    public static Task Handle(
        SendTeamMemberAdded message,
        IEmailTemplateProvider templates,
        ISendEmail sender,
        IProcessedEventStore processedEvents,
        NotificationMetrics metrics,
        ILogger<SendTeamMemberAdded> logger,
        CancellationToken ct
    ) =>
        processedEvents.SendOnceAsync(
            message,
            logger,
            metrics,
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
