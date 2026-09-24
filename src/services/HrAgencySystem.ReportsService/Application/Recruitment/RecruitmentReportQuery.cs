using Dapper;
using HrAgencySystem.Reports.ReadModel;
using HrAgencySystem.ReportsService.Contracts;
using Npgsql;

namespace HrAgencySystem.ReportsService.Application.Recruitment;

/// <summary>
/// One organization's recruitment report, in one round trip. Every statement filters on the
/// organization first - it leads every index of the <c>reports</c> schema.
/// </summary>
public sealed class RecruitmentReportQuery(NpgsqlDataSource dataSource)
{
    private const string JobPostsTable = $"{ReportsSchema.Name}.{ReportsSchema.Tables.JobPosts}";
    private const string ApplicationsTable =
        $"{ReportsSchema.Name}.{ReportsSchema.Tables.Applications}";
    private const string InterviewsTable =
        $"{ReportsSchema.Name}.{ReportsSchema.Tables.Interviews}";

    /*
     * The funnel counts "reached at least": an application moved straight from screening to an
     * offer never got an interview date, yet it is past the interview stage. Without the coalesce
     * chain the funnel could widen on the way down.
     */
    private const string Sql = $"""
        select
            (select count(*) from {JobPostsTable}
              where organization_id = @org and first_published_at >= @from and first_published_at < @to)
                as "JobPostsPublished",
            (select count(*) from {ApplicationsTable}
              where organization_id = @org and created_at >= @from and created_at < @to)
                as "Applications",
            (select count(*) from {InterviewsTable}
              where organization_id = @org and created_at >= @from and created_at < @to)
                as "InterviewsScheduled",
            (select count(*) from {InterviewsTable}
              where organization_id = @org and completed_at >= @from and completed_at < @to)
                as "InterviewsHeld",
            (select count(*) from {ApplicationsTable}
              where organization_id = @org and offer_at >= @from and offer_at < @to)
                as "Offers",
            (select count(*) from {ApplicationsTable}
              where organization_id = @org and hired_at >= @from and hired_at < @to)
                as "Hires";

        select
            count(*) as "Applied",
            count(coalesce(screening_at, interview_at, assessment_at, offer_at, hired_at)) as "Screening",
            count(coalesce(interview_at, assessment_at, offer_at, hired_at)) as "Interview",
            count(coalesce(assessment_at, offer_at, hired_at)) as "Assessment",
            count(coalesce(offer_at, hired_at)) as "Offer",
            count(hired_at) as "Hired",
            count(rejected_at) as "Rejected",
            count(withdrawn_at) as "Withdrawn"
        from {ApplicationsTable}
        where organization_id = @org and created_at >= @from and created_at < @to;

        select date_trunc('month', created_at at time zone 'UTC') as "Month", 'applications' as "Metric", count(*) as "Count"
          from {ApplicationsTable}
         where organization_id = @org and created_at >= @from and created_at < @to
         group by 1
        union all
        select date_trunc('month', created_at at time zone 'UTC'), 'interviews', count(*)
          from {InterviewsTable}
         where organization_id = @org and created_at >= @from and created_at < @to
         group by 1
        union all
        select date_trunc('month', offer_at at time zone 'UTC'), 'offers', count(*)
          from {ApplicationsTable}
         where organization_id = @org and offer_at >= @from and offer_at < @to
         group by 1
        union all
        select date_trunc('month', hired_at at time zone 'UTC'), 'hires', count(*)
          from {ApplicationsTable}
         where organization_id = @org and hired_at >= @from and hired_at < @to
         group by 1;

        select source, count(*) as "Applications"
          from {ApplicationsTable}
         where organization_id = @org and created_at >= @from and created_at < @to
         group by source
         order by count(*) desc, source;
        """;

    public async Task<RecruitmentReport> RunAsync(
        Guid organizationId,
        ReportPeriod period,
        CancellationToken ct
    )
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        await using var results = await connection.QueryMultipleAsync(
            new CommandDefinition(
                Sql,
                new
                {
                    org = organizationId,
                    from = period.StartsAt,
                    to = period.EndsBefore,
                },
                cancellationToken: ct
            )
        );

        var totals = await results.ReadSingleAsync<TotalsRow>();
        var funnel = await results.ReadSingleAsync<FunnelRow>();
        var monthly = (await results.ReadAsync<MonthlyRow>()).ToList();
        var sources = await results.ReadAsync<SourceRow>();

        return new RecruitmentReport(
            period.FromText,
            period.ToText,
            new RecruitmentTotals(
                (int)totals.JobPostsPublished,
                (int)totals.Applications,
                (int)totals.InterviewsScheduled,
                (int)totals.InterviewsHeld,
                (int)totals.Offers,
                (int)totals.Hires
            ),
            new RecruitmentFunnel(
                (int)funnel.Applied,
                (int)funnel.Screening,
                (int)funnel.Interview,
                (int)funnel.Assessment,
                (int)funnel.Offer,
                (int)funnel.Hired,
                (int)funnel.Rejected,
                (int)funnel.Withdrawn,
                funnel.Applied == 0 ? null : Math.Round((decimal)funnel.Hired / funnel.Applied, 4)
            ),
            [.. Months(period, monthly)],
            [.. sources.Select(row => new SourceCount(row.Source, (int)row.Applications))]
        );
    }

    /// <summary>Every month of the period, including the silent ones - a gap is a zero, not a hole.</summary>
    private static IEnumerable<RecruitmentMonth> Months(ReportPeriod period, List<MonthlyRow> rows)
    {
        foreach (var month in period.Months())
        {
            var start = month.ToDateTime(TimeOnly.MinValue);

            int Count(string metric) =>
                (int)rows.Where(r => r.Month == start && r.Metric == metric).Sum(r => r.Count);

            yield return new RecruitmentMonth(
                ReportPeriod.Write(month),
                Count("applications"),
                Count("interviews"),
                Count("offers"),
                Count("hires")
            );
        }
    }

    // Rows materialized by Dapper, which sets the init accessors through reflection.
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    private sealed class TotalsRow
    {
        public long JobPostsPublished { get; init; }
        public long Applications { get; init; }
        public long InterviewsScheduled { get; init; }
        public long InterviewsHeld { get; init; }
        public long Offers { get; init; }
        public long Hires { get; init; }
    }

    private sealed class FunnelRow
    {
        public long Applied { get; init; }
        public long Screening { get; init; }
        public long Interview { get; init; }
        public long Assessment { get; init; }
        public long Offer { get; init; }
        public long Hired { get; init; }
        public long Rejected { get; init; }
        public long Withdrawn { get; init; }
    }

    private sealed class MonthlyRow
    {
        public DateTime Month { get; init; }
        public string Metric { get; init; } = "";
        public long Count { get; init; }
    }

    private sealed class SourceRow
    {
        public string Source { get; init; } = "";
        public long Applications { get; init; }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Local
}
