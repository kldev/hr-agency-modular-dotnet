using Npgsql;

namespace HrAgencySystem.Api.Infrastructure;

internal static class SetupNpgsqlDataSource
{
    internal static void AddDataSource(this IServiceCollection services)
    {
        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var connectionString =
                configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException(
                    "Postgres connection string is not configured.");

            return NpgsqlDataSource.Create(connectionString);
        });
    }
}