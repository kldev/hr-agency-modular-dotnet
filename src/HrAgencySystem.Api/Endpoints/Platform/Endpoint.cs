using HrAgencySystem.PlatformSeeder;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Platform;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/development/seed", Handler)
            .ExcludeFromDescription()
            .AllowAnonymous().WithRequestTimeout(TimeSpan.FromMinutes(5));
        
        endpoints.MapGet("/api/development/seed/{type}", HandlerApplicants)
            .ExcludeFromDescription()
            .AllowAnonymous().WithRequestTimeout(TimeSpan.FromMinutes(5));
        
        endpoints.MapGet("/api/development/seed-sales", HandlerSales)
            .ExcludeFromDescription()
            .AllowAnonymous().WithRequestTimeout(TimeSpan.FromMinutes(5));
    }

    private static async Task<IResult> Handler(IPlatformSeeder seeder, ILogger<IPlatformSeeder> logger)
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
    
    private static async Task<IResult> HandlerApplicants(IPlatformSeeder seeder,
        [FromQuery] int count = 100, string type = "random")
    {
        if (type == "show")
        {
            await seeder.SeedShowcase();
            return TypedResults.Text("Seed showcase applicants completed");    
        }
        await seeder.SeedApplicants(count);
        return TypedResults.Text("Seed applicants completed");
    }
    
    private static async Task<IResult> HandlerSales(IPlatformSalesSeeder seeder, int count = 500, string slug = "hr-agency", CancellationToken ct = default)
    {
        
        await seeder.Seed(Math.Clamp(count,50, 2000), slug, ct);
        
        return TypedResults.Text("Seed completed");
    }
}