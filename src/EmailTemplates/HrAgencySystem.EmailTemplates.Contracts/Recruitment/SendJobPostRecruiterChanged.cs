namespace HrAgencySystem.EmailTemplates.Contracts.Recruitment;

/// <summary>
/// Sent only when somebody hands a job post over to another recruiter; reassigning a post to
/// yourself is not news to anybody.
/// </summary>
public sealed record SendJobPostRecruiterChanged(
    Guid EventId,
    string Source,
    Guid JobPostId,
    string JobPostTitle,
    string RecruiterEmail,
    string RecruiterFullname,
    string ChangedByFullname
) : IEmailTemplateContract;
