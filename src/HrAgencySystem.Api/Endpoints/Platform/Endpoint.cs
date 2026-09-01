using HrAgencySystem.PlatformSeeder;

namespace HrAgencySystem.Api.Endpoints.Platform;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/development/seed", Handler).ExcludeFromDescription().AllowAnonymous();
    }

    private static async Task<IResult> Handler(IPlatformSeeder seeder)
    {
        await seeder.Seed();
        return TypedResults.Text("Seed completed");
    }
}