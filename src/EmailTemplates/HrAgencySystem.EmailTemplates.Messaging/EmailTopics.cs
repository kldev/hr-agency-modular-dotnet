namespace HrAgencySystem.EmailTemplates.Messaging;

/// <summary>
/// Routing keys of the mail topic exchange. A topic is part of the wire contract, so it is spelled
/// out here rather than derived from a CLR type name that is free to change.
/// </summary>
public static class EmailTopics
{
    public const string JobApplicationCreated = "recruitment.application.created";
    public const string JobPostRecruiterChanged = "recruitment.jobpost.recruiter-changed";
    public const string PasswordReset = "identity.password.reset";
    public const string OpportunityCreated = "sales.opportunity.created";
    public const string OpportunityResponsibleChanged = "sales.opportunity.responsible-changed";
    public const string TeamMemberAdded = "teams.member.added";
    public const string TeamMemberRoleChanged = "teams.member.role-changed";

    public const string RecruitmentPattern = "recruitment.#";
    public const string IdentityPattern = "identity.#";
    public const string SalesPattern = "sales.#";
    public const string TeamsPattern = "teams.#";
}
