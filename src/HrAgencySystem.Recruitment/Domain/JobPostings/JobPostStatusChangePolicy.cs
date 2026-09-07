namespace HrAgencySystem.Recruitment.Domain.JobPostings;

public static class JobPostStatusChangePolicy
{
    public static bool Allow(
        JobPostStatus oldStatus,
        JobPostStatus newStatus)
    {
        if (oldStatus == newStatus)
            return false;

        if (IsFinal(oldStatus))
            return false;

        return (oldStatus, newStatus) switch
        {
            (JobPostStatus.Draft, JobPostStatus.Published) => true,

            (JobPostStatus.Published, JobPostStatus.Archived) => true,
            (JobPostStatus.Published, JobPostStatus.Closed) => true,
            (JobPostStatus.Archived, JobPostStatus.Published) => true,

            _ => false
        };
    }

    public static bool IsFinal(JobPostStatus status)
    {
        return status is
            JobPostStatus.Closed;
    }
}