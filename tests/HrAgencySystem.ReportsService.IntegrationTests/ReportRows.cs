using Dapper;
using HrAgencySystem.Reports.ReadModel;
using Npgsql;

namespace HrAgencySystem.ReportsService.IntegrationTests;

/// <summary>Writes rows the way the projections would, straight into the contract tables.</summary>
internal sealed class ReportRows(NpgsqlDataSource dataSource)
{
    private const string Schema = ReportsSchema.Name;

    public async Task OrganizationAsync(Guid id, string name, DateTimeOffset at)
    {
        await Execute(
            $"""
            insert into {Schema}.{ReportsSchema.Tables.Organizations}
                (id, organization_id, name, slug, created_at, updated_at)
            values (@id, @id, @name, @slug, @at, @at)
            """,
            new
            {
                id,
                name,
                slug = name.ToLowerInvariant().Replace(' ', '-'),
                at,
            }
        );
    }

    public async Task JobPostAsync(Guid org, DateTimeOffset createdAt, DateTimeOffset? publishedAt)
    {
        await Execute(
            $"""
            insert into {Schema}.{ReportsSchema.Tables.JobPosts}
                (id, organization_id, company_id, is_published, created_at, first_published_at, updated_at)
            values (@id, @org, @company, @published, @createdAt, @publishedAt, @createdAt)
            """,
            new
            {
                id = Guid.NewGuid(),
                org,
                company = Guid.NewGuid(),
                published = publishedAt is not null,
                createdAt,
                publishedAt,
            }
        );
    }

    public async Task ApplicationAsync(
        Guid org,
        DateTimeOffset createdAt,
        string status = "Applied",
        string source = "JustJoinIt",
        DateTimeOffset? screeningAt = null,
        DateTimeOffset? interviewAt = null,
        DateTimeOffset? offerAt = null,
        DateTimeOffset? hiredAt = null,
        DateTimeOffset? rejectedAt = null
    )
    {
        var updatedAt = new[] { createdAt, screeningAt, interviewAt, offerAt, hiredAt, rejectedAt }
            .OfType<DateTimeOffset>()
            .Max();

        await Execute(
            $"""
            insert into {Schema}.{ReportsSchema.Tables.Applications}
                (id, organization_id, job_post_id, source, status, created_at, screening_at,
                 interview_at, assessment_at, offer_at, hired_at, rejected_at, withdrawn_at, updated_at)
            values (@id, @org, @jobPost, @source, @status, @createdAt, @screeningAt,
                    @interviewAt, null, @offerAt, @hiredAt, @rejectedAt, null, @updatedAt)
            """,
            new
            {
                id = Guid.NewGuid(),
                org,
                jobPost = Guid.NewGuid(),
                source,
                status,
                createdAt,
                screeningAt,
                interviewAt,
                offerAt,
                hiredAt,
                rejectedAt,
                updatedAt,
            }
        );
    }

    public async Task InterviewAsync(
        Guid org,
        DateTimeOffset createdAt,
        DateTimeOffset? completedAt = null
    )
    {
        await Execute(
            $"""
            insert into {Schema}.{ReportsSchema.Tables.Interviews}
                (id, organization_id, job_application_id, status, created_at, completed_at, updated_at)
            values (@id, @org, @application, @status, @createdAt, @completedAt, @createdAt)
            """,
            new
            {
                id = Guid.NewGuid(),
                org,
                application = Guid.NewGuid(),
                status = completedAt is null ? "Planned" : "Completed",
                createdAt,
                completedAt,
            }
        );
    }

    public async Task ProjectAsync(
        Guid org,
        DateTimeOffset createdAt,
        string status,
        DateTimeOffset? wentLiveAt,
        DateTimeOffset? updatedAt = null
    )
    {
        await Execute(
            $"""
            insert into {Schema}.{ReportsSchema.Tables.Projects}
                (id, organization_id, company_id, engagement_type, country_code, status,
                 created_at, went_live_at, updated_at)
            values (@id, @org, @company, 'Outsourcing', 'PL', @status, @createdAt, @wentLiveAt, @updatedAt)
            """,
            new
            {
                id = Guid.NewGuid(),
                org,
                company = Guid.NewGuid(),
                status,
                createdAt,
                wentLiveAt,
                updatedAt = updatedAt ?? wentLiveAt ?? createdAt,
            }
        );
    }

    private async Task Execute(string sql, object parameters)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql, parameters);
    }
}
