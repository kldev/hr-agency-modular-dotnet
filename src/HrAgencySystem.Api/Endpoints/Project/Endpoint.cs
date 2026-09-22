using HrAgencySystem.Compliance;

namespace HrAgencySystem.Api.Endpoints.Project;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        // No role gate: running a project is open to any authenticated member of the organization.
        // The fallback-deny policy already keeps anonymous callers out.
        var group = endpoints.MapGroup("").WithTags("Sales - Projects");

        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeStatus.Map(group);
        Maps.MapChangeLegalEntity.Map(group);
        Maps.MapAssignTeam.Map(group);
        Maps.MapAssignContact.Map(group);
        Maps.MapRemoveContact.Map(group);
        Maps.MapSetEmails.Map(group);
        Maps.MapRecordContract.Map(group);
        Maps.MapChangeContractStatus.Map(group);
        Maps.MapRecordCompliance.Map(group);
        Maps.MapComplianceCatalogue.Map(group);
        Maps.MapAttachDocument.Map(group);
        Maps.MapUpdateDocument.Map(group);
        Maps.MapRemoveDocument.Map(group);
        Maps.MapDownloadDocument.Map(group);
    }
}
