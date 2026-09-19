using HrAgencySystem.EmailTemplates.Contracts.Sales;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendOpportunityCreatedHandler
{
    public static Task Handle(
        SendOpportunityCreated message,
        ILogger<SendOpportunityCreated> logger
    )
    {
        logger.LogInformation(
            "Opportunity {OpportunityTitle} for {CompanyName}: mailing {ResponsiblePersonEmail}",
            message.OpportunityTitle,
            message.CompanyName,
            message.ResponsiblePersonEmail
        );

        return Task.CompletedTask;
    }
}
