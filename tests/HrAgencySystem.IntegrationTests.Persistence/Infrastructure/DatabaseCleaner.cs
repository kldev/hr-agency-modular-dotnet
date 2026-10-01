using HrAgencySystem.Organization.Infrastructure.Persistence;
using HrAgencySystem.Recruitment.Projections;
using Npgsql;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

/// <summary>
/// The part of the shared cleaner these tests use. The shared one knows the documents of every
/// module and would pull the whole API into a project that needs three of them.
/// </summary>
public sealed class DatabaseCleaner(string connectionString)
{
    public Task CleanOrganizationReservation() =>
        CleanTable<OrganizationSlugReservation>("organization");

    public Task CleanInterviews() => CleanTable<InterviewProjection>("recruitment");

    private async Task CleanTable<T>(string schema)
    {
        try
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            var command = dataSource.CreateCommand(
                $"truncate table {schema}.mt_doc_{typeof(T).Name.ToLower()}"
            );
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // ignored - the table does not exist before the first store creates it
        }
    }
}
