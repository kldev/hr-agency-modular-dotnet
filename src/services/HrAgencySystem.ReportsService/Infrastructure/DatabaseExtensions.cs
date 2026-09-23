using Npgsql;

namespace HrAgencySystem.ReportsService.Infrastructure;

public static class DatabaseExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// A plain data source on the platform database. This service reads the <c>reports</c>
        /// schema and nothing else: no Marten, no event store, no migrations - the API's projections
        /// create and fill the tables.
        /// </summary>
        public void SetupReportsDatabase(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");

            // Queries alias their columns to the row properties, so no global Dapper naming switch.
            services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        }
    }
}
