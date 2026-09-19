using HrAgencySystem.EmailTemplates.Contracts.Identity;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendPasswordResetHandler
{
    public static Task Handle(SendPasswordReset message, ILogger<SendPasswordReset> logger)
    {
        logger.LogInformation(
            "Password reset valid for {ExpiresInMinutes} min: mailing {RecipientEmail}",
            message.ExpiresInMinutes,
            message.RecipientEmail
        );

        return Task.CompletedTask;
    }
}
