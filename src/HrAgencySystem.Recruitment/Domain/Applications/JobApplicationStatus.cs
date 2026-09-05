namespace HrAgencySystem.Recruitment.Domain.Applications;

public enum JobApplicationStatus
{
    /**
  * Candidate has submitted an application for the job posting.
  */
    Applied,

    /**
     * Application is being reviewed by a recruiter.
     */
    Screening,

    /**
     * Candidate is participating in an interview process.
     */
    Interview,

    /**
     * Candidate is completing an assessment, test or other evaluation.
     */
    Assessment,
    
    /**
     * An employment offer has been made to the candidate.
     */
    Offer,

    /**
     * Candidate has accepted the offer and has been hired.
     */
    Hired,

    /**
     * Application has been rejected by the recruitment team.
     */
    Rejected,

    /**
     * Candidate has withdrawn their application.
     */
    Withdrawn,
}