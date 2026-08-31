using Endpoint = HrAgencySystem.Api.Endpoints.Company.Endpoint;

namespace HrAgencySystem.Api.Endpoints;

public static class MapEndpoints
{
    public static void MapApplicationEndpoints(this WebApplication app)
    {
        Endpoint.Map(app);
        Organization.Endpoint.Map(app);
        Owner.Endpoint.Map(app);
        User.Endpoint.Map(app);

        MapPlatformSeeder(app);
    }

    private static void MapPlatformSeeder(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            Platform.Endpoint.Map(app);
        }
    }
}