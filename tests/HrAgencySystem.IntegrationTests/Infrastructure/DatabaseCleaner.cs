using Npgsql;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public sealed class DatabaseCleaner(string connectionString)
{
    public async Task CleanOrganizationReservation()
    {
        try
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            var command = dataSource.CreateCommand("truncate table org.mt_doc_organizationslugreservation");
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // ignored
        }
    }
}