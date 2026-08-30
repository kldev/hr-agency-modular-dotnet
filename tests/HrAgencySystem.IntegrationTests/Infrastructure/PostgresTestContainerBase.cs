using Testcontainers.PostgreSql;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public abstract class PostgresTestContainerBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("hr_agency_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    /// <summary>
    ///     Override this method to run migrations and seed test data
    /// </summary>
    protected virtual Task InitializeDatabaseAsync()
    {
        return Task.CompletedTask;
    }
}