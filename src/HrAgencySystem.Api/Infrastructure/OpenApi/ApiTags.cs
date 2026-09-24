namespace HrAgencySystem.Api.Infrastructure.OpenApi;

/// <summary>
/// Every tag the API groups its operations under, with the sentence Scalar shows above the group.
/// One place for both, so an endpoint group names its tag through a constant and a tag cannot
/// exist without a description.
/// <para>
/// The names are the wire contract of the generated front end client - orval lays its endpoint
/// folders out by tag - so renaming one moves files in <c>frontend/src/api</c>.
/// </para>
/// </summary>
internal static class ApiTags
{
    public const string Auth = "Auth";
    public const string Owner = "Owner";
    public const string Organization = "Organization";
    public const string Users = "Users";
    public const string Suggestion = "Suggestion";

    public const string Company = "Company";
    public const string CompanyContacts = "Company contacts";

    public const string SalesOpportunity = "Sales - Opportunity";
    public const string SalesJobDescription = "Sales - Job Description";
    public const string SalesProjects = "Sales - Projects";
    public const string SalesTasks = "Sales - Tasks";

    public const string RecruitmentJobPosting = "Recruitment - Job Posting";
    public const string RecruitmentCandidates = "Recruitment - Candidates";
    public const string RecruitmentJobApplications = "Recruitment - Job Applications";
    public const string RecruitmentInterviews = "Recruitment - Interviews";

    public const string EmploymentPositions = "Employment - Positions";
    public const string EmploymentWorkers = "Employment - Workers";
    public const string EmploymentAssignments = "Employment - Assignments";

    public const string AgencyStructure = "Agency - Structure";
    public const string AgencyEmployment = "Agency - Employment";
    public const string AgencyTimeSheets = "Agency - Time sheets";
    public const string AgencyTeams = "Agency - Teams";
    public const string AgencyLegalEntities = "Agency - Legal entities";

    public const string FormsDefinitions = "Forms - Definitions";
    public const string FormsSystemFields = "Forms - System fields";
    public const string FormsResponses = "Forms - Responses";

    public const string Reports = "Reports";

    public const string Internal = "Internal";

    /// <summary>In the order Scalar lists them: the way a sale turns into people at work.</summary>
    public static readonly IReadOnlyList<(string Name, string Description)> All =
    [
        (
            Auth,
            "Signing in and out, refreshing a session, resetting a password, and an admin acting as somebody else. The only group that works without a bearer token."
        ),
        (
            Owner,
            "The platform owner: its own accounts, and the service API keys programs such as the public job board present. Requires the platform Owner role."
        ),
        (
            Organization,
            "The agencies on the platform - each one a tenant - and their first users. Managed by the platform owner, not by the agencies themselves."
        ),
        (
            Users,
            "The people of one agency: accounts, roles, profiles and avatars. Everything here is scoped to the caller's organization."
        ),
        (
            Suggestion,
            "Typeahead lookups for pickers - users, companies, job posts, projects. Short answers meant for a dropdown, not for listing."
        ),
        (
            Company,
            "The agency's clients: companies it sells to and recruits for, with the profile a contract needs before a project can go live."
        ),
        (CompanyContacts, "The people at a client company the agency talks to."),
        (
            SalesOpportunity,
            "The sales pipeline: opportunities from first contact to won or lost, and the activities and follow-up actions that move them along."
        ),
        (
            SalesTasks,
            "A person's own to-do list: tasks for a client company, optionally within one of its deals, done with one click and undone the same way."
        ),
        (
            SalesJobDescription,
            "Internal descriptions of the positions a client wants filled. One job description is one position; the candidate-facing text lives in job posts."
        ),
        (
            SalesProjects,
            "Delivering what was sold: the project, its contract, contacts, documents and per-country compliance."
        ),
        (
            RecruitmentJobPosting,
            "Candidate-facing job posts: the polished text of a job description, its status and the channels it was published to. A description can have several posts, e.g. one per language."
        ),
        (
            RecruitmentCandidates,
            "People who applied or were sourced, independent of any one job post. One candidate per e-mail address within an agency."
        ),
        (
            RecruitmentJobApplications,
            "A candidate applying to one job post: its status through the recruitment pipeline, notes and tags."
        ),
        (
            RecruitmentInterviews,
            "Interviews scheduled for job applications: when, in what format and with whom."
        ),
        (
            EmploymentPositions,
            "The roles on a project the agency staffs people into: contract type, proposed rate, schedule and workplace."
        ),
        (
            EmploymentWorkers,
            "The register of people the agency sends to clients: the person, their documents and work permits, and their pipeline from recruitment to employed."
        ),
        (
            EmploymentAssignments,
            "One posting of one worker to one project for one period, with its documents and the per-person compliance it triggers (A1, local contract)."
        ),
        (
            AgencyStructure,
            "The agency's own org chart - boards, departments, sections, their members and heads. Supervisors are worked out from it, never stored."
        ),
        (
            AgencyEmployment,
            "What the agency's own people work on: contract type, period, weekly hours and rate. It decides who owes a time sheet."
        ),
        (
            AgencyTimeSheets,
            "Monthly time sheets of the agency's own people: filling days in, sending for approval, approval by the supervisor from the chart, settlement by payroll and its export."
        ),
        (AgencyTeams, "Recruitment teams and who belongs to them."),
        (
            FormsDefinitions,
            "Forms, documents and surveys the agency defines without a developer: pages of fields, a working draft and the published versions people fill in."
        ),
        (
            FormsSystemFields,
            "The organization's catalogue of fields describing a person - PESEL, phone, citizenship - defined once and shown by any form that asks for them."
        ),
        (
            FormsResponses,
            "Forms filled in for a worker: drafts, submission, corrections with a reason, and the version each response was given to."
        ),
        (
            AgencyLegalEntities,
            "The companies the agency trades and posts people through, with their registrations and bank accounts."
        ),
        (
            Internal,
            "Routes for the agency's own programs, behind a service API key rather than a user token. Not part of the public document."
        ),
    ];
}
