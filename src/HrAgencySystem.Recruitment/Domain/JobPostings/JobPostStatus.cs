using JetBrains.Annotations;

namespace HrAgencySystem.Recruitment.Domain.JobPostings;

public enum JobPostStatus
{
    /**
    * Posting is being prepared and is not publicly visible.
    */
    Draft,

    /**
     * Posting is currently active and available to candidates.
     */
    Published,

    /**
     * Posting is no longer accepting new applications.
     */
    Closed,

    /**
     * Posting has been permanently archived.
     */
    Archived,
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum JobPostStatusApi
{
    Published,
    Closed,
    Archived,
}

public static class JobPostStatusApiExtensions
{
    extension(JobPostStatusApi value)
    {
        public JobPostStatus ToDomain() => Enum.Parse<JobPostStatus>(value.ToString());
    }
}
