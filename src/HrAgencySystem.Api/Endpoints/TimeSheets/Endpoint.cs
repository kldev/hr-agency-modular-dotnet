using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.TimeSheets;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.AgencyTimeSheets);

        Maps.MapGetMine.Map(group);
        Maps.MapGetTeam.Map(group);
        Maps.MapGetSettlement.Map(group);
        Maps.MapExportSettlement.Map(group);
        Maps.MapGetSheet.Map(group);

        Maps.MapSaveWorkDay.Map(group);
        Maps.MapRemoveWorkDay.Map(group);
        Maps.MapSubmit.Map(group);
        Maps.MapApprove.Map(group);
        Maps.MapReturn.Map(group);
        Maps.MapSettle.Map(group);
        Maps.MapComment.Map(group);
    }
}
