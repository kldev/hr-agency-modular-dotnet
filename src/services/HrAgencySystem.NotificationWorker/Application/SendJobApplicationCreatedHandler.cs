using HrAgencySystem.EmailTemplates.Contracts.Recruitment;

namespace HrAgencySystem.NotificationWorker.Application;

public static class SendJobApplicationCreatedHandler
{
    public static Task Handle(SendJobApplicationCreated command, CancellationToken ct)
    {
        Console.WriteLine($"Sending job application created: {command.JobApplicationId}");
        Console.WriteLine($"Applicant email {command.ApplicantEmail}");
        return Task.CompletedTask;
    }
}
