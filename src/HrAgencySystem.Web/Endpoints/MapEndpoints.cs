using Endpoint = HrAgencySystem.Web.Endpoints.Public.Endpoint;

namespace HrAgencySystem.Web.Endpoints;

public static class MapEndpoints
{
    public static void MapApplicationEndpoints(this WebApplication app)
    {
        Endpoint.Map(app);
    }
}