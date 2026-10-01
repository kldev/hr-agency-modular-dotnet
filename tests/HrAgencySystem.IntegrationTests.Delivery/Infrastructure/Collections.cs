namespace HrAgencySystem.IntegrationTests.Infrastructure;

// xUnit discovers collection definitions only in the assembly that holds the tests, so every
// integration test project declares its own over the fixture from HrAgencySystem.IntegrationTests.Shared.

[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<IntegrationEnvironment>
{
    public const string Name = "Integration";
}
