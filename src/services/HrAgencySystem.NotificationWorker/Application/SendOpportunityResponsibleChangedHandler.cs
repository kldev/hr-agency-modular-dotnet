using HrAgencySystem.EmailTemplates.Contracts.Sales;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityResponsibleChangedHandler
{
    public static Task Handle(
        SendOpportunityResponsibleChanged message,
        ILogger<SendOpportunityResponsibleChanged> logger
    )
    {
        logger.LogInformation(
            "Opportunity {OpportunityTitle} handed over by {ChangedByFullname}: mailing {ResponsibleEmail}",
            message.OpportunityTitle,
            message.ChangedByFullname,
            message.ResponsibleEmail
        );

        return Task.CompletedTask;
    }
}
