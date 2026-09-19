namespace HrAgencySystem.EmailTemplates.Contracts.Recruitment;

public sealed record SendJobApplicationCreated(
    Guid EventId,
    string Source,
    Guid JobApplicationId,
    Guid JobPostId,
    string JobPostTitle,
    string ApplicantEmail,
    string ApplicantFullname,
    string ApplicantPhone,
    string RecruiterFullname,
    // Deep link into the portal; when empty the mail simply drops its call to action.
    string ApplicationUrl = ""
) : IEmailTemplateContract;
