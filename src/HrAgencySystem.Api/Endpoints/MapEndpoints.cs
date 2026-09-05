namespace HrAgencySystem.Api.Endpoints;

public static class MapEndpoints
{
    public static void MapApplicationEndpoints(this WebApplication app)
    {
        Auth.Endpoint.Map(app);
        Company.Endpoint.Map(app);
        Organization.Endpoint.Map(app);
        Owner.Endpoint.Map(app);
        User.Endpoint.Map(app);
        JobDescription.Endpoint.Map(app);
        Suggestion.Endpoint.Map(app);
        JobPosting.Endpoint.Map(app);
        Candidate.Endpoint.Map(app);
        JobApplication.Endpoint.Map(app);

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