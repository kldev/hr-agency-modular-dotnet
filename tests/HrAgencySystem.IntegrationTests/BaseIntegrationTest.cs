using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Port;
using Npgsql;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests;

public abstract class BaseIntegrationTest
{
    protected ITestOutputHelper OutputHelper { get; }
    protected String  ConnectionString { get; init; }

    protected BaseIntegrationTest(ApiPostgresTestContainer container, ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        var factory = new ApiApplicationFactory(container.ConnectionString).WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var service = services.SingleOrDefault(z => z.ServiceType == typeof(IOrganizationChecker));
                outputHelper.WriteLine("Using IOrganizationChecker factory: " + service!.GetType().AssemblyQualifiedName);
            });
        });
        factory.StartServer();
        Client = factory.CreateClient();
        ConnectionString = container.ConnectionString;
    }
    
    protected HttpClient Client { get; }
    
    protected async Task<OrganizationCreated> CreateOrganizationAsync(
        string name = "HR Agency",
        string slug = "hr-agency")
    {
        var request = new CreateOrganization(
            name,
            slug,
            null);

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<OrganizationCreated>(
            OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    protected void CleanOrganizationReservation()
    {
        try
        {
            using var dataSource = NpgsqlDataSource.Create(ConnectionString);
            var command = dataSource.CreateCommand("truncate table org.mt_doc_organizationslugreservation");
            command.ExecuteNonQuery();
        }
        catch
        {
            // ignored
        }
    }
}