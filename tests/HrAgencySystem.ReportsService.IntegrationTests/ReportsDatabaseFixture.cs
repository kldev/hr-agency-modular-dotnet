using Dapper;
using HrAgencySystem.Reports.ReadModel;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HrAgencySystem.ReportsService.IntegrationTests;

/// <summary>
/// A database holding only the <c>reports</c> schema, created from the read model contexts
/// themselves - the contract the API's projections write to. The queries are tested against the
/// table shape, not against a hand-written copy of it that could drift.
/// </summary>
public sealed class ReportsDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("hr_reports")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();
        DataSource = NpgsqlDataSource.Create(connectionString);

        await using var connection = await DataSource.OpenConnectionAsync();
        await connection.ExecuteAsync($"create schema if not exists {ReportsSchema.Name}");

        foreach (var script in CreateScripts(connectionString))
        {
            await connection.ExecuteAsync(script);
        }
    }

    public async Task CleanAsync()
    {
        await using var connection = await DataSource.OpenConnectionAsync();
        await connection.ExecuteAsync($"truncate table {string.Join(", ", AllTables())}");
    }

    public async Task DisposeAsync()
    {
        await DataSource.DisposeAsync();
        await _container.DisposeAsync();
    }

    private static IEnumerable<string> AllTables() =>
        new[]
        {
            ReportsSchema.Tables.Organizations,
            ReportsSchema.Tables.JobPosts,
            ReportsSchema.Tables.Applications,
            ReportsSchema.Tables.Interviews,
            ReportsSchema.Tables.Projects,
        }.Select(table => $"{ReportsSchema.Name}.{table}");

    private static IEnumerable<string> CreateScripts(string connectionString)
    {
        yield return Script<OrganizationsReportDbContext>(connectionString, o => new(o));
        yield return Script<JobPostsReportDbContext>(connectionString, o => new(o));
        yield return Script<ApplicationsReportDbContext>(connectionString, o => new(o));
        yield return Script<InterviewsReportDbContext>(connectionString, o => new(o));
        yield return Script<ProjectsReportDbContext>(connectionString, o => new(o));
    }

    private static string Script<TContext>(
        string connectionString,
        Func<DbContextOptions<TContext>, TContext> create
    )
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>().UseNpgsql(connectionString).Options;
        using var context = create(options);

        return context.Database.GenerateCreateScript();
    }
}

[CollectionDefinition(Name)]
public sealed class ReportsDatabaseCollection : ICollectionFixture<ReportsDatabaseFixture>
{
    public const string Name = "ReportsDatabase";
}
