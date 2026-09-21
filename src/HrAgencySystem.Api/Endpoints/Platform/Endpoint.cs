using HrAgencySystem.PlatformSeeder;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Platform;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.Development.Seed, Handler)
            .ExcludeFromDescription()
            .AllowAnonymous()
            .WithRequestTimeout(TimeSpan.FromMinutes(5));

        endpoints
            .MapGet(ApiEndpoints.Development.SeedType, HandlerApplicants)
            .ExcludeFromDescription()
            .AllowAnonymous()
            .WithRequestTimeout(TimeSpan.FromMinutes(5));

        endpoints
            .MapGet(ApiEndpoints.Development.SeedSales, HandlerSales)
            .ExcludeFromDescription()
            .AllowAnonymous()
            .WithRequestTimeout(TimeSpan.FromMinutes(5));
    }

    private static async Task<IResult> Handler(
        IPlatformSeeder seeder,
        ILogger<IPlatformSeeder> logger
    )
    {
        try
        {
            await seeder.Seed();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while seeding the platform");
            return TypedResults.InternalServerError(ex.Message);
        }

        return TypedResults.Text("Seed completed");
    }

    private static async Task<IResult> HandlerApplicants(
        IPlatformSeeder seeder,
        [FromQuery] int count = 100,
        string type = "random",
        [FromQuery] string slug = "hr-agency"
    )
    {
        if (type == "show")
        {
            await seeder.SeedShowcase();
            return TypedResults.Text("Seed showcase applicants completed");
        }

        // Legal entities, projects, workers and assignments for an agency that already exists.
        if (type == "delivery")
        {
            await seeder.SeedDelivery(slug);
            return TypedResults.Text($"Seed delivery completed for '{slug}'");
        }

        var clamp = Math.Clamp(count, 1, 200);
        await seeder.SeedApplicants(clamp);
        return TypedResults.Text($"Seed applicants completed. Count: {clamp}");
    }

    private static async Task<IResult> HandlerSales(
        IPlatformSalesSeeder seeder,
        int count = 500,
        string slug = "hr-agency",
        CancellationToken ct = default
    )
    {
        var clamp = Math.Clamp(count, 1, 2000);
        await seeder.Seed(clamp, slug, ct);

        return TypedResults.Text($"Seed completed. Count: {clamp}");
    }
}
