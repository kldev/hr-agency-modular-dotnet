using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.Worker;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        // No role gate: the register is read and kept by recruitment, HR, legalisation and
        // operations in turn, and the fallback-deny policy already keeps anonymous callers out.
        var group = endpoints.MapGroup("").WithTags(ApiTags.EmploymentWorkers);

        Maps.MapRegister.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeStatus.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapAttachDocument.Map(group);
        Maps.MapUpdateDocument.Map(group);
        Maps.MapRemoveDocument.Map(group);
        Maps.MapDownloadDocument.Map(group);
        Maps.MapRecordAuthorisation.Map(group);
        Maps.MapRemoveAuthorisation.Map(group);
    }
}
