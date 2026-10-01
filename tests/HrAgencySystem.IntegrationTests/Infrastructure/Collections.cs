namespace HrAgencySystem.IntegrationTests.Infrastructure;

// xUnit discovers collection definitions only in the assembly that holds the tests, so every
// integration test project declares its own over the fixtures from HrAgencySystem.IntegrationTests.Shared.

[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<IntegrationEnvironment>
{
    public const string Name = "Integration";
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
