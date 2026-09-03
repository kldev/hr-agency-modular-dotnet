namespace HrAgencySystem.Recruitment.Domain.Candidate;

public enum CandidateSource
{
    /// <summary>
    /// Candidate applied through company's career page.
    /// </summary>
    CareerPage,

    /// <summary>
    /// Candidate came from Pracuj.pl.
    /// </summary>
    PracujPl,

    /// <summary>
    /// Candidate came from OLX Praca.
    /// </summary>
    Olx,

    /// <summary>
    /// Candidate came from Praca.pl.
    /// </summary>
    PracaPl,

    /// <summary>
    /// Candidate came from RocketJobs.pl.
    /// </summary>
    RocketJobs,

    /// <summary>
    /// Candidate came from Just Join IT.
    /// </summary>
    JustJoinIt,

    /// <summary>
    /// Candidate came from No Fluff Jobs.
    /// </summary>
    NoFluffJobs,

    /// <summary>
    /// Candidate came from LinkedIn.
    /// </summary>
    LinkedIn,

    /// <summary>
    /// Candidate came from Indeed.
    /// </summary>
    Indeed,

    /// <summary>
    /// Candidate was referred by another person.
    /// </summary>
    Referral,

    /// <summary>
    /// Candidate was sourced directly by a recruiter.
    /// </summary>
    DirectSourcing,

    /// <summary>
    /// Candidate already existed in the recruitment database.
    /// </summary>
    InternalDatabase,

    /// <summary>
    /// Candidate was provided by another recruitment agency.
    /// </summary>
    RecruitmentAgency,

    /// <summary>
    /// Candidate contacted the agency directly,
    /// without applying through a published job posting.
    /// </summary>
    DirectApplication,

    /// <summary>
    /// Facebook campaign.
    /// </summary>
    Facebook,

    /// <summary>
    /// Source is known but does not match a predefined value.
    /// </summary>
    Other,

    /// <summary>
    /// Candidate came from a direct source.
    /// </summary>
    Direct
}
