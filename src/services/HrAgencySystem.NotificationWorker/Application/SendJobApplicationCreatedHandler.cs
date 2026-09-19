using HrAgencySystem.EmailTemplates.Contracts.Recruitment;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobApplicationCreatedHandler
{
    public static Task Handle(
        SendJobApplicationCreated message,
        ILogger<SendJobApplicationCreated> logger
    )
    {
        logger.LogInformation(
            "Job application {JobApplicationId} for {JobPostTitle}: mailing {ApplicantEmail}",
            message.JobApplicationId,
            message.JobPostTitle,
            message.ApplicantEmail
        );

        return Task.CompletedTask;
    }
}
