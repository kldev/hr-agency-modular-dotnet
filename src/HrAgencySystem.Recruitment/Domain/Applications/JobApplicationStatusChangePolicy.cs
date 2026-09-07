namespace HrAgencySystem.Recruitment.Domain.Applications;

public static class JobApplicationStatusChangePolicy
{
    public static bool Allow(
        JobApplicationStatus currentStatus,
        JobApplicationStatus newStatus)
    {
        if (currentStatus == newStatus)
            return false;

        if ((newStatus == JobApplicationStatus.Rejected || newStatus == JobApplicationStatus.Withdrawn) &&
            IsFinal(currentStatus))
        {
            return false;
        }



        return (currentStatus, newStatus) switch
        {
            (JobApplicationStatus.Applied, JobApplicationStatus.Screening) => true,

            (JobApplicationStatus.Screening, JobApplicationStatus.Assessment) => true,
            (JobApplicationStatus.Screening, JobApplicationStatus.Interview) => true,

            (JobApplicationStatus.Assessment, JobApplicationStatus.Interview) => true,
            (JobApplicationStatus.Assessment, JobApplicationStatus.Offer) => true,

            (JobApplicationStatus.Interview, JobApplicationStatus.Assessment) => true,
            (JobApplicationStatus.Interview, JobApplicationStatus.Offer) => true,

            (_, JobApplicationStatus.Hired) => true,
            (_, JobApplicationStatus.Rejected) => true,
            (_, JobApplicationStatus.Withdrawn) => true,

            _ => false
        };
    }

    private static bool IsFinal(JobApplicationStatus status)
    {
        return status is
            JobApplicationStatus.Hired or
            JobApplicationStatus.Rejected or
            JobApplicationStatus.Withdrawn;
    }
}
