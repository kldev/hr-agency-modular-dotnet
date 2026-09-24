namespace HrAgencySystem.Api.Endpoints;

public static class MapEndpoints
{
    public static void MapApplicationEndpoints(this WebApplication app)
    {
        Auth.Endpoint.Map(app);
        Owner.Endpoint.Map(app);
        User.Endpoint.Map(app);
        Company.Endpoint.Map(app);
        CompanyContacts.Endpoint.Map(app);
        Sales.Endpoint.Map(app);
        SalesOpportunity.Endpoint.Map(app);
        SalesFollowUpAction.Endpoint.Map(app);
        Project.Endpoint.Map(app);
        JobDescription.Endpoint.Map(app);

        JobPosting.Endpoint.Map(app);
        Candidate.Endpoint.Map(app);
        JobApplication.Endpoint.Map(app);
        Public.Endpoint.Map(app);
        Internal.Endpoint.Map(app);
        Interviews.Endpoint.Map(app);
        Suggestion.Endpoint.Map(app);
        Organization.Endpoint.Map(app);

        Position.Endpoint.Map(app);
        Worker.Endpoint.Map(app);
        Assignment.Endpoint.Map(app);
        OrgStructure.Endpoint.Map(app);
        AgencyEmployment.Endpoint.Map(app);
        Teams.Endpoint.Map(app);
        TimeSheets.Endpoint.Map(app);
        LegalEntity.Endpoint.Map(app);
        Forms.Endpoint.Map(app);
        SystemFields.Endpoint.Map(app);
        FormResponses.Endpoint.Map(app);
        Reports.Endpoint.Map(app);

        MapPlatformSeeder(app);
    }

    private static void MapPlatformSeeder(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "docker")
        {
            Platform.Endpoint.Map(app);
        }
    }
}
