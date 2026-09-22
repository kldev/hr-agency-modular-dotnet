namespace HrAgencySystem.Api.Endpoints.AgencyEmployment;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Agency - Employment");

        Maps.MapList.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapStart.Map(group);
        Maps.MapChangeTerms.Map(group);
        Maps.MapEnd.Map(group);
    }
}
