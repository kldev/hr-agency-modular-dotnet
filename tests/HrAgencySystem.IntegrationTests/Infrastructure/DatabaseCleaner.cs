using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Company.Infrastructure.Persistence;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.Identity.Documents;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.JobDescription.Projections;
using HrAgencySystem.LegalEntities.Infrastructure.Persistence;
using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.Organization.Infrastructure.Persistence;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.Recruitment.Infrastructure.Persistence;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.Recruitment.Projections.Timeline;
using HrAgencySystem.Sales.Documents;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.Teams.Infrastructure.Persistence;
using HrAgencySystem.Teams.Projections;
using HrAgencySystem.Workers.Infrastructure.Persistence;
using HrAgencySystem.Workers.Projections;
using Npgsql;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public sealed class DatabaseCleaner(string connectionString)
{
    private async Task CleanTable<T>(string schema)
    {
        var tableName = $"truncate table {schema}.mt_doc_{typeof(T).Name.ToLower()}";
        await TruncateTable(tableName);
    }

    public async Task CleanOwnerEmailReservation()
    {
        await CleanTable<OwnerEmailReservation>("identity");
    }

    public async Task CleanUserEmailReservation()
    {
        await CleanTable<UserEmailReservation>("identity");
    }

    public async Task CleanRefreshTokens()
    {
        await CleanTable<RefreshToken>("identity");
    }

    public async Task CleanUsers()
    {
        await CleanTable<UserEmailReservation>("identity");
        await CleanTable<UserProjection>("identity");
        await CleanTable<UserProfile>("identity");
    }

    public async Task CleanCompany()
    {
        await CleanTable<CompanyTaxIdReservation>("company");
        await CleanTable<CompanyProjection>("company");
    }

    public async Task CleanOrganizationReservation()
    {
        await CleanTable<OrganizationSlugReservation>("organization");
    }

    public async Task CleanOrganizations()
    {
        await CleanTable<OrganizationSlugReservation>("organization");
        await CleanTable<HrAgencySystem.Organization.Projections.OrganizationProjection>(
            "organization"
        );
    }

    public async Task CleanCompanyTaxIds()
    {
        await CleanTable<CompanyTaxIdReservation>("company");
    }

    public async Task CleanJobDescriptions()
    {
        await CleanTable<JobDescriptionProjection>("job_description");
        await CleanTable<JdStatusChangeHistory>("job_description");
    }

    public async Task CleanProjects()
    {
        await CleanTable<ProjectProjection>("projects");
        await CleanTable<ProjectPositionProjection>("projects");
    }

    public async Task CleanLegalEntities()
    {
        await CleanTable<LegalEntityProjection>("legal_entities");
        await CleanTable<LegalEntityTaxIdReservation>("legal_entities");
    }

    public async Task CleanWorkers()
    {
        await CleanTable<WorkerProjection>("workers");
        await CleanTable<AssignmentProjection>("workers");
        await CleanTable<WorkerIdentityDocumentReservation>("workers");
        await CleanTable<WorkerEmailReservation>("workers");
    }

    public async Task CleanForms()
    {
        await CleanTable<FormDefinitionProjection>("forms");
        await CleanTable<FormResponseProjection>("forms");
        await CleanTable<FormVersion>("forms");
        await CleanTable<SubjectProfile>("forms");
        await CleanTable<FormCodeReservation>("forms");
    }

    public async Task CleanOrgStructure()
    {
        await CleanTable<OrgStructureProjection>("agency");
    }

    public async Task CleanTimeRecords()
    {
        await CleanTable<AgencyEmploymentProjection>("agency");
        await CleanTable<TimeSheetProjection>("agency");
    }

    public async Task CleanTeams()
    {
        await CleanTable<TeamProjection>("teams");
        await CleanTable<TeamMembershipReservation>("teams");
    }

    public async Task CleanInterviews()
    {
        await CleanTable<InterviewProjection>("recruitment");
    }

    public async Task CleanCandidates()
    {
        await CleanTable<CandidateEmailReservation>("recruitment");
        await CleanTable<CandidateProjection>("recruitment");
    }

    public async Task CleanApiKeys()
    {
        await CleanTable<ServiceApiKey>("identity");
    }

    public async Task CleanJobApplications()
    {
        await CleanTable<JobApplicationProjection>("recruitment");
    }

    public async Task CleanTimeline()
    {
        await CleanTable<TimelineEntry>("recruitment");
    }

    public async Task CleanJobPostFeedRows()
    {
        await TruncateTable("truncate table feeds.job_posts");
    }

    public async Task CleanReports()
    {
        await TruncateTable(
            "truncate table reports.organizations, reports.job_posts, reports.applications, "
                + "reports.interviews, reports.projects"
        );
    }

    public async Task CleanSales()
    {
        await CleanTable<ActivityProjection>("sales");
        await CleanTable<OpportunityProjection>("sales");
        await CleanTable<FollowUpAction>("sales");
    }

    private async Task TruncateTable(string sql)
    {
        try
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            var command = dataSource.CreateCommand(sql);
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // ignored
        }
    }
}
