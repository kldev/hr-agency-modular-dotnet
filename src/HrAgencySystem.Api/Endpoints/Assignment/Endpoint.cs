namespace HrAgencySystem.Api.Endpoints.Assignment;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Employment - Assignments");

        Maps.MapPlan.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeStatus.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapComplianceCatalogue.Map(group);
        Maps.MapRecordCompliance.Map(group);
        Maps.MapAttachDocument.Map(group);
        Maps.MapUpdateDocument.Map(group);
        Maps.MapRemoveDocument.Map(group);
        Maps.MapDownloadDocument.Map(group);
    }
}
