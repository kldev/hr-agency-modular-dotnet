namespace HrAgencySystem.Recruitment.Domain.Posting;

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
    Archived
}