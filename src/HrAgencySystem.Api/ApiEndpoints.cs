namespace HrAgencySystem.Api;

internal static class ApiEndpoints
{
    private const string Base = "/api";

    public const string Root = "/";
    public const string Health = "/healthz";

    internal static class Auth
    {
        public const string Login = $"{Base}/auth/login";
        public const string Refresh = $"{Base}/auth/refresh";
        public const string Logout = $"{Base}/auth/logout";
        public const string RequestPasswordReset = $"{Base}/auth/password-reset";
        public const string CompletePasswordReset = $"{Base}/auth/password-reset/confirm";
        public const string Current = $"{Base}/user/me";
        public const string OwnerLogin = $"{Base}/owner/login";
        public const string CurrentOwner = $"{Base}/owner/me";
    }

    internal static class Users
    {
        private const string UsersBase = $"{Base}/users";

        public const string Create = UsersBase;
        public const string Slice = UsersBase;
        public const string Get = $"{UsersBase}/{{userId:guid}}";
        public const string Update = $"{UsersBase}/{{userId:guid}}";
        public const string ChangeRole = $"{UsersBase}/{{userId:guid}}/role";
        public const string ChangePassword = $"{UsersBase}/me/password";

        // "me" never matches the :guid constraint on Get/Update, so the literal and the parameter
        // route can share a segment - the same arrangement ChangePassword already relies on.
        public const string Me = $"{UsersBase}/me";
        public const string Avatar = $"{UsersBase}/me/avatar";
    }

    internal static class Owners
    {
        private const string OwnersBase = $"{Base}/owners";

        public const string Create = OwnersBase;
        public const string GetAll = OwnersBase;
        public const string Get = $"{OwnersBase}/{{ownerId:guid}}";
    }

    internal static class Organizations
    {
        private const string OrganizationBase = $"{Base}/organization";

        public const string Create = OrganizationBase;
        public const string Slice = OrganizationBase;
        public const string Get = $"{OrganizationBase}/{{organizationId:guid}}";
        public const string Update = $"{OrganizationBase}/{{organizationId:guid}}";
        public const string UpdateSlug = $"{OrganizationBase}/{{organizationId}}/slug";
        public const string GetBySlug = $"{OrganizationBase}/{{slug}}";

        public const string Users = $"{OrganizationBase}/users";
        public const string CreateUser = $"{OrganizationBase}/users";
        public const string UpdateUser = $"{OrganizationBase}/users/{{userId:guid}}";
    }

    internal static class Companies
    {
        private const string CompaniesBase = $"{Base}/companies";

        public const string Create = CompaniesBase;
        public const string Slice = CompaniesBase;
        public const string Get = $"{CompaniesBase}/{{companyId:guid}}";
        public const string Update = $"{CompaniesBase}/{{companyId:guid}}";
        public const string GetByTaxId = $"{CompaniesBase}/find-by-tax/{{taxId}}";
        public const string Contacts = $"{CompaniesBase}/{{companyId:guid}}/contacts";
        public const string CompleteProfile = $"{CompaniesBase}/{{companyId:guid}}/profile";
    }

    internal static class CompanyContacts
    {
        private const string ContactsBase = $"{Base}/company-contacts";

        public const string Create = $"{ContactsBase}/{{companyId:guid}}";
        public const string Get = $"{ContactsBase}/{{contactId:guid}}";
        public const string Update = $"{ContactsBase}/{{contactId:guid}}";
        public const string Delete = $"{ContactsBase}/{{contactId:guid}}";
    }

    internal static class Sales
    {
        private const string SalesBase = $"{Base}/sales";

        public const string LogActivity = $"{SalesBase}/activity";
        public const string ActivitySlice = $"{SalesBase}/activities";

        internal static class Opportunities
        {
            private const string OpportunityBase = $"{SalesBase}/opportunity";

            public const string Create = OpportunityBase;
            public const string Slice = OpportunityBase;
            public const string Get = $"{OpportunityBase}/{{opportunityId:guid}}";
            public const string Update = $"{OpportunityBase}/{{opportunityId:guid}}";
            public const string ChangeStage = $"{OpportunityBase}/{{opportunityId:guid}}/stage";
            public const string ChangeResponsible =
                $"{OpportunityBase}/{{opportunityId:guid}}/responsible";
            public const string Totals = $"{OpportunityBase}/totals";
            public const string ResponsibleTotals = $"{OpportunityBase}/totals-responsible";
        }

        internal static class FollowUpActions
        {
            private const string FollowUpBase = $"{SalesBase}/follow-up";

            public const string Create = FollowUpBase;
            public const string Slice = FollowUpBase;
            public const string Get = $"{FollowUpBase}/{{followUpActionId:guid}}";
            public const string Update = $"{FollowUpBase}/{{followUpActionId:guid}}";
        }
    }

    internal static class JobDescriptions
    {
        private const string JobDescriptionBase = $"{Base}/job-description";

        public const string Create = JobDescriptionBase;
        public const string Slice = JobDescriptionBase;
        public const string StatusHistory = $"{JobDescriptionBase}/status";
        public const string Get = $"{JobDescriptionBase}/{{jobDescriptionId:guid}}";
        public const string Update = $"{JobDescriptionBase}/{{jobDescriptionId}}";
        public const string UpdateStatus =
            $"{JobDescriptionBase}/{{jobDescriptionId:guid}}/{{status}}";
        public const string AssignRecruiter =
            $"{JobDescriptionBase}/{{jobDescriptionId:guid}}/assign-recruiter";
    }

    internal static class Recruitment
    {
        private const string RecruitmentBase = $"{Base}/recruitment";

        internal static class JobPosts
        {
            private const string JobPostBase = $"{RecruitmentBase}/job-posting";

            public const string Create = JobPostBase;
            public const string Slice = JobPostBase;
            public const string Get = $"{JobPostBase}/{{jobPostId:guid}}";
            public const string Update = $"{JobPostBase}/{{jobPostId}}";
            public const string ChangeStatus = $"{JobPostBase}/{{jobPostId}}/status";
            public const string ChangeRecruiter =
                $"{JobPostBase}/{{jobPostId:guid}}/change-recruiter";
            public const string PostToChannel = $"{JobPostBase}/{{jobPostId:guid}}/channel";
            public const string ApplyTo = $"{JobPostBase}/{{jobPostId:guid}}/apply";
        }

        internal static class Candidates
        {
            private const string CandidateBase = $"{RecruitmentBase}/candidates";

            public const string Create = CandidateBase;
            public const string Slice = CandidateBase;
            public const string Get = $"{CandidateBase}/{{candidateId:guid}}";
            public const string Update = $"{CandidateBase}/{{candidateId:guid}}";
            public const string Tag = $"{CandidateBase}/{{candidateId:guid}}/tag";
            public const string TagList = $"{CandidateBase}/{{candidateId:guid}}/tag-list";
            public const string RemoveTagList = $"{CandidateBase}/{{candidateId:guid}}/tag/remove";
            public const string RemoveTag =
                $"{CandidateBase}/{{candidateId:guid}}/tag/{{tagId:guid}}";
        }

        internal static class JobApplications
        {
            private const string JobApplicationBase = $"{RecruitmentBase}/job-applications";

            public const string Slice = JobApplicationBase;
            public const string Get = $"{JobApplicationBase}/{{jobApplicationId:guid}}";
            public const string Update = $"{JobApplicationBase}/{{jobApplicationId:guid}}";
            public const string ChangeStatus = $"{JobApplicationBase}/{{jobApplicationId}}/status";
            public const string Notes = $"{JobApplicationBase}/{{jobApplicationId:guid}}/notes";
            public const string CreateNote = $"{JobApplicationBase}/{{applicationId:guid}}/note";
            public const string DeleteNote =
                $"{JobApplicationBase}/{{applicationId:guid}}/note/{{noteId:guid}}";
            public const string Tag = $"{JobApplicationBase}/{{applicationId:guid}}/tag";
            public const string TagList = $"{JobApplicationBase}/{{applicationId:guid}}/tag-list";
            public const string RemoveTagList =
                $"{JobApplicationBase}/{{applicationId:guid}}/tag/remove";
            public const string RemoveTag =
                $"{JobApplicationBase}/{{applicationId:guid}}/tag/{{tagId:guid}}";
        }
    }

    internal static class Interviews
    {
        private const string InterviewBase = $"{Base}/interviews";

        public const string Schedule = $"{InterviewBase}/schedule";
        public const string Slice = InterviewBase;
        public const string Range = $"{InterviewBase}/range";
        public const string Get = $"{InterviewBase}/{{interviewId}}";
        public const string Reschedule = $"{InterviewBase}/{{interviewId}}/reschedule";
        public const string ChangeStatus = $"{InterviewBase}/{{interviewId}}/status";
        public const string ChangeFormat = $"{InterviewBase}/{{interviewId}}/format";
        public const string ChangeInterviewer = $"{InterviewBase}/{{interviewId}}/interviewer";
    }

    internal static class LegalEntities
    {
        private const string LegalEntitiesBase = $"{Base}/legal-entities";

        public const string Create = LegalEntitiesBase;
        public const string Slice = LegalEntitiesBase;
        public const string Get = $"{LegalEntitiesBase}/{{legalEntityId:guid}}";
        public const string Update = $"{LegalEntitiesBase}/{{legalEntityId:guid}}";
        public const string Close = $"{LegalEntitiesBase}/{{legalEntityId:guid}}/close";
    }

    internal static class Teams
    {
        private const string TeamsBase = $"{Base}/teams";

        public const string Create = TeamsBase;
        public const string Slice = TeamsBase;
        public const string Get = $"{TeamsBase}/{{teamId:guid}}";
        public const string Rename = $"{TeamsBase}/{{teamId:guid}}/name";
        public const string AddMember = $"{TeamsBase}/{{teamId:guid}}/members";
        public const string RemoveMember = $"{TeamsBase}/{{teamId:guid}}/members/{{userId:guid}}";
        public const string ChangeMemberRole =
            $"{TeamsBase}/{{teamId:guid}}/members/{{userId:guid}}/role";
    }

    internal static class Projects
    {
        private const string ProjectsBase = $"{Base}/projects";

        public const string Create = ProjectsBase;
        public const string Slice = ProjectsBase;
        public const string Get = $"{ProjectsBase}/{{projectId:guid}}";
        public const string Update = $"{ProjectsBase}/{{projectId:guid}}";
        public const string ChangeStatus = $"{ProjectsBase}/{{projectId:guid}}/status";
        public const string ChangeLegalEntity = $"{ProjectsBase}/{{projectId:guid}}/legal-entity";
        public const string AssignTeam = $"{ProjectsBase}/{{projectId:guid}}/team";
        public const string AssignContact = $"{ProjectsBase}/{{projectId:guid}}/contacts/{{role}}";
        public const string RemoveContact = $"{ProjectsBase}/{{projectId:guid}}/contacts/{{role}}";
        public const string SetEmails = $"{ProjectsBase}/{{projectId:guid}}/emails/{{purpose}}";
        public const string RecordContract = $"{ProjectsBase}/{{projectId:guid}}/contract";
        public const string ChangeContractStatus =
            $"{ProjectsBase}/{{projectId:guid}}/contract/status";
        public const string RecordCompliance =
            $"{ProjectsBase}/{{projectId:guid}}/compliance/{{requirement}}";
        public const string ComplianceCatalogue =
            $"{ProjectsBase}/{{projectId:guid}}/compliance/catalogue";
        public const string AttachDocument = $"{ProjectsBase}/{{projectId:guid}}/documents";
        public const string UpdateDocument =
            $"{ProjectsBase}/{{projectId:guid}}/documents/{{documentId:guid}}";
        public const string RemoveDocument =
            $"{ProjectsBase}/{{projectId:guid}}/documents/{{documentId:guid}}";
        public const string DownloadDocument =
            $"{ProjectsBase}/{{projectId:guid}}/documents/{{documentId:guid}}/content";
    }

    internal static class Suggestions
    {
        private const string SuggestionBase = $"{Base}/suggestion";

        public const string Companies = $"{SuggestionBase}/companies";
        public const string Company = $"{SuggestionBase}/companies/{{companyId:guid}}";
        public const string CompanyContacts = $"{SuggestionBase}/company-contacts";
        public const string JobPosts = $"{SuggestionBase}/job-posts";
        public const string Tags = $"{SuggestionBase}/tags";
        public const string Users = $"{SuggestionBase}/users";
        public const string User = $"{SuggestionBase}/users/{{userId:guid}}";
        public const string Projects = $"{SuggestionBase}/projects";
        public const string Teams = $"{SuggestionBase}/teams";
        public const string Team = $"{SuggestionBase}/teams/{{teamId:guid}}";
    }

    // Anonymous, excluded from OpenAPI: the feed files served next to the public job board.
    internal static class Public
    {
        public const string Group = "p";

        public const string JobsXml = "{slug}/jobs.xml";
        public const string JobsJson = "{slug}/jobs.json";
    }

    // Registered only in the Development and docker environments.
    internal static class Development
    {
        private const string DevelopmentBase = $"{Base}/development";

        public const string Seed = $"{DevelopmentBase}/seed";
        public const string SeedType = $"{DevelopmentBase}/seed/{{type}}";
        public const string SeedSales = $"{DevelopmentBase}/seed-sales";
    }
}
