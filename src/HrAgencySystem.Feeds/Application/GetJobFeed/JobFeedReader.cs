using Dapper;
using HrAgencySystem.Feeds.ReadModel;
using Npgsql;

namespace HrAgencySystem.Feeds.Application.GetJobFeed;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class JobFeedReader(NpgsqlDataSource dataSource) : IJobFeedReader
{
    private const string SelectPublishedSql = """
        select id               as "Id",
               organization_id  as "OrganizationId",
               is_published     as "IsPublished",
               title            as "Title",
               summary          as "Summary",
               description      as "Description",
               responsibilities as "Responsibilities",
               requirements     as "Requirements",
               skills           as "Skills",
               location         as "Location",
               language_code    as "LanguageCode",
               country_code     as "CountryCode",
               employment_type  as "EmploymentType",
               work_mode        as "WorkMode",
               currency_code    as "CurrencyCode",
               salary_min       as "SalaryMin",
               salary_max       as "SalaryMax",
               posting_slug     as "PostingSlug",
               created_at       as "CreatedAt",
               updated_at       as "UpdatedAt"
        from feeds.job_posts
        where organization_id = @organizationId
          and is_published = true
        order by created_at desc
        """;

    public async Task<IReadOnlyList<JobPostFeedRow>> GetJobsFeed(
        Guid organizationId,
        CancellationToken ct
    )
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        var command = new CommandDefinition(
            SelectPublishedSql,
            new { organizationId },
            cancellationToken: ct
        );

        var rows = await connection.QueryAsync<JobPostFeedRow>(command);

        return [.. rows];
    }
}
