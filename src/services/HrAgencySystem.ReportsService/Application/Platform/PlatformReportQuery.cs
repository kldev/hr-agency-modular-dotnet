using Dapper;
using HrAgencySystem.Reports.ReadModel;
using HrAgencySystem.ReportsService.Contracts;
using Npgsql;

namespace HrAgencySystem.ReportsService.Application.Platform;

/// <summary>
/// Every organization side by side. Correlated subqueries per organization rather than joins:
/// five fact tables joined on one key multiply rows before they are counted, and the tenant list
/// is short enough that one index probe per cell is the cheap way.
/// </summary>
public sealed class PlatformReportQuery(NpgsqlDataSource dataSource)
{
    private const string Organizations =
        $"{ReportsSchema.Name}.{ReportsSchema.Tables.Organizations}";
    private const string JobPosts = $"{ReportsSchema.Name}.{ReportsSchema.Tables.JobPosts}";
    private const string Applications = $"{ReportsSchema.Name}.{ReportsSchema.Tables.Applications}";
    private const string Interviews = $"{ReportsSchema.Name}.{ReportsSchema.Tables.Interviews}";
    private const string Projects = $"{ReportsSchema.Name}.{ReportsSchema.Tables.Projects}";

    // greatest() skips nulls in PostgreSQL, so a tenant without projects still gets its last activity.
    private const string Sql = $"""
        select
            o.id as "OrganizationId",
            o.name,
            o.slug,
            o.created_at as "CreatedAt",
            (select count(*) from {JobPosts} x
              where x.organization_id = o.id and x.first_published_at >= @from and x.first_published_at < @to)
                as "JobPostsPublished",
            (select count(*) from {Applications} x
              where x.organization_id = o.id and x.created_at >= @from and x.created_at < @to)
                as "Applications",
            (select count(*) from {Interviews} x
              where x.organization_id = o.id and x.created_at >= @from and x.created_at < @to)
                as "InterviewsScheduled",
            (select count(*) from {Applications} x
              where x.organization_id = o.id and x.offer_at >= @from and x.offer_at < @to)
                as "Offers",
            (select count(*) from {Applications} x
              where x.organization_id = o.id and x.hired_at >= @from and x.hired_at < @to)
                as "Hires",
            (select count(*) from {Projects} x
              where x.organization_id = o.id and x.went_live_at >= @from and x.went_live_at < @to)
                as "ProjectsWentLive",
            (select count(*) from {Projects} x
              where x.organization_id = o.id and x.status = 'Active')
                as "ProjectsActive",
            greatest(
                o.updated_at,
                (select max(updated_at) from {JobPosts} x where x.organization_id = o.id),
                (select max(updated_at) from {Applications} x where x.organization_id = o.id),
                (select max(updated_at) from {Interviews} x where x.organization_id = o.id),
                (select max(updated_at) from {Projects} x where x.organization_id = o.id)
            ) as "LastActivityAt"
        from {Organizations} o
        order by "LastActivityAt" desc nulls last, o.name
        """;

    public async Task<PlatformReport> RunAsync(ReportPeriod period, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        var rows = (
            await connection.QueryAsync<OrganizationRow>(
                new CommandDefinition(
                    Sql,
                    new { from = period.StartsAt, to = period.EndsBefore },
                    cancellationToken: ct
                )
            )
        ).ToList();

        var organizations = rows.Select(row => new OrganizationActivity(
                row.OrganizationId,
                row.Name,
                row.Slug,
                row.CreatedAt,
                (int)row.JobPostsPublished,
                (int)row.Applications,
                (int)row.InterviewsScheduled,
                (int)row.Offers,
                (int)row.Hires,
                (int)row.ProjectsWentLive,
                (int)row.ProjectsActive,
                row.LastActivityAt
            ))
            .ToList();

        return new PlatformReport(
            period.FromText,
            period.ToText,
            Totals(organizations),
            organizations
        );
    }

    /// <summary>
    /// An organization counts as active when anything happened in it during the period - a tenant
    /// that only logged in is not activity this schema can see.
    /// </summary>
    internal static PlatformTotals Totals(IReadOnlyList<OrganizationActivity> organizations) =>
        new(
            organizations.Count,
            organizations.Count(o =>
                o.JobPostsPublished
                    + o.Applications
                    + o.InterviewsScheduled
                    + o.Hires
                    + o.ProjectsWentLive
                > 0
            ),
            organizations.Sum(o => o.JobPostsPublished),
            organizations.Sum(o => o.Applications),
            organizations.Sum(o => o.InterviewsScheduled),
            organizations.Sum(o => o.Hires),
            organizations.Sum(o => o.ProjectsWentLive)
        );

    private sealed class OrganizationRow
    {
        public Guid OrganizationId { get; init; }
        public string Name { get; init; } = "";
        public string Slug { get; init; } = "";
        public DateTimeOffset CreatedAt { get; init; }
        public long JobPostsPublished { get; init; }
        public long Applications { get; init; }
        public long InterviewsScheduled { get; init; }
        public long Offers { get; init; }
        public long Hires { get; init; }
        public long ProjectsWentLive { get; init; }
        public long ProjectsActive { get; init; }
        public DateTimeOffset? LastActivityAt { get; init; }
    }
}
