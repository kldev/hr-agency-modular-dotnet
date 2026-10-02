using JetBrains.Annotations;

namespace HrAgencySystem.Recruitment.Domain.JobPostings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum PostingChannelType
{
    /**
 * Company's own career page.
 */
    CareerPage,

    /**
     * Pracuj.pl
     */
    PracujPl,

    /**
     * OLX Praca
     */
    Olx,

    /**
     * Praca.pl
     */
    PracaPl,

    /**
     * RocketJobs.pl
     */
    Rocketjobs,

    /**
     * Just Join IT
     */
    JustJoinIt,

    /**
     * No Fluff Jobs
     */
    NoFluffJobs,

    /**
     * LinkedIn Jobs
     */
    Linkedin,

    /**
     * Indeed
     */
    Indeed,

    /**
     * Other external job board.
     */
    Other,
}
