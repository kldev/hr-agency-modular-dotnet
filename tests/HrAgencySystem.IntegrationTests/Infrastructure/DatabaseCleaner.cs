using HrAgencySystem.Company.Infrastructure.Persistence;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.Organization.Infrastructure.Persistence;
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
    
    public async Task CleanUsers()
    {
        await CleanTable<UserEmailReservation>("identity");
        await CleanTable<UserProjection>("identity");
    }
    

    public async Task CleanOrganizationReservation()
    {
        await CleanTable<OrganizationSlugReservation>("organization");
    }

    public async Task CleanCompanyTaxIds()
    {
        await CleanTable<CompanyTaxIdReservation>("company");
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