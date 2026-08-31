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
    }
}