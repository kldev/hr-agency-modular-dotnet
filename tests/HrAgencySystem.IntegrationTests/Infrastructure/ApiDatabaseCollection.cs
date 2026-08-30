namespace HrAgencySystem.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public class ApiDatabaseCollection : ICollectionFixture<ApiPostgresTestContainer>
{
    public const string Name = "ApiDatabase";
}