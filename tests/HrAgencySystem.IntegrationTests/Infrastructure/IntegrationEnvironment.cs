using Testcontainers.PostgreSql;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class IntegrationEnvironment : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("hr_agency_test")
        .WithUsername("hr_agency")
        .WithPassword("hr_agency")
        .Build();

    private ApiApplicationFactory Factory { get; set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public DatabaseCleaner Cleaner { get; private set; } = null!;

    private string ConnectionString =>
        _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Cleaner = new DatabaseCleaner(ConnectionString);

        Factory = new ApiApplicationFactory(ConnectionString);

        Factory.StartServer();

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        await Factory.DisposeAsync();

        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<IntegrationEnvironment>
{
    public const string Name = "Integration";
}