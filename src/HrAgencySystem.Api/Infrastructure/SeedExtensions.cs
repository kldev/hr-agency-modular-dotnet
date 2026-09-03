using HrAgencySystem.SharedKernel.Port;

namespace HrAgencySystem.Api.Infrastructure;

public static class SeedExtensions
{
    public static async Task SeedAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        
        var seeders = scope.ServiceProvider
            .GetServices<ISeeder>();

        foreach (var seeder in seeders)
            await seeder.SeedAsync(cancellationToken);
    }
}