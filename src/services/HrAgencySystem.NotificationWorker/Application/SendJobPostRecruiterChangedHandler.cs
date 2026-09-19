using HrAgencySystem.EmailTemplates.Contracts.Recruitment;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobPostRecruiterChangedHandler
{
    public static Task Handle(
        SendJobPostRecruiterChanged message,
        ILogger<SendJobPostRecruiterChanged> logger
    )
    {
        logger.LogInformation(
            "Job post {JobPostTitle} handed over by {ChangedByFullname}: mailing {RecruiterEmail}",
            message.JobPostTitle,
            message.ChangedByFullname,
            message.RecruiterEmail
        );

        return Task.CompletedTask;
    }
}
